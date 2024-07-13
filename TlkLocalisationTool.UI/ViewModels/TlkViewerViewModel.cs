using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TlkLocalisationTool.Logic.Services.Interfaces;
using TlkLocalisationTool.Shared.Settings;
using TlkLocalisationTool.UI.Constants;
using TlkLocalisationTool.UI.Extensions;
using TlkLocalisationTool.UI.Models;
using TlkLocalisationTool.UI.Parameters;
using TlkLocalisationTool.UI.Resources;
using TlkLocalisationTool.UI.Utils;

namespace TlkLocalisationTool.UI.ViewModels;

public class TlkViewerViewModel : ViewModelBase
{
    private readonly AppSettings _appSettings;
    private readonly ITlkReader _tlkReader;
    private readonly ITlkWriter _tlkWriter;
    private readonly ILookupService _lookupService;
    private readonly IJsonReader _jsonReader;
    private readonly IJsonWriter _jsonWriter;

    private TlkEntryModel[] _unfilteredEntries;
    private string[] _originalEntries;
    private TlkEntryModel _previousSelectedEntry;
    private SaveFileDialog _saveFileDialog;

    private TlkEntryModel _selectedEntry;

    private Command _filterCommand;
    private Command _editCommand;
    private Command _selectContextCommand;
    private Command _settingsCommand;
    private Command _generateLookupFileCommand;
    private Command _exportLocalisedCommand;
    private Command _exportOriginalCommand;
    private Command _saveCommand;

    public TlkViewerViewModel(AppSettings appSettings, ITlkReader tlkReader, ITlkWriter tlkWriter, ILookupService lookupService, IJsonReader jsonReader, IJsonWriter jsonWriter)
    {
        _appSettings = appSettings;
        _tlkReader = tlkReader;
        _tlkWriter = tlkWriter;
        _lookupService = lookupService;
        _jsonReader = jsonReader;
        _jsonWriter = jsonWriter;
    }

    public ObservableCollection<TlkEntryModel> Entries { get; } = [];

    public string Filter { get; set; }

    public TlkEntryModel SelectedEntry
    {
        get => _selectedEntry;
        set
        {
           _previousSelectedEntry = _selectedEntry;
            _selectedEntry = value;
            OnPropertyChanged();
        }
    }

    public Command FilterCommand => _filterCommand ??= new Command(_ => FilterEntries());

    public Command EditCommand => _editCommand ??= new Command(_ => ShowEntryEditor(), _ => SelectedEntry != null);

    public Command SelectContextCommand => _selectContextCommand ??= new Command(_ => ShowContextSelector(), _ => SelectedEntry?.IsContextAvailable == true);

    public Command SettingsCommand => _settingsCommand ??= new Command(async _ => await ShowSettingsEditorAndReloadEntries());

    public Command GenerateLookupFileCommand => _generateLookupFileCommand ??= new Command(async _ => await GenerateLookupFile());

    public Command ExportLocalisedCommand => _exportLocalisedCommand ??= new Command(
        async _ => await ExportEntries(_unfilteredEntries.Select(x => x.Value).ToArray()),
        _ => _unfilteredEntries != null);

    public Command ExportOriginalCommand => _exportOriginalCommand ??= new Command(async _ => await ExportEntries(_originalEntries), _ => _originalEntries != null);

    public Command SaveCommand => _saveCommand ??= new Command(async _ => await SaveLocalisedFile(), _ => _unfilteredEntries != null);

    public override async Task Init()
    {
        Title = Strings.TlkViewer_Title;

        var areSettigsValid = await ValidateSettings();
        if (!areSettigsValid)
        {
            return;
        }

        if (Directory.Exists(_appSettings.ExtractedGameFilesPath) && !File.Exists(DataConstants.LookupDataFileName))
        {
            var messageBoxResult = MessageBox.Show(Strings.TlkViewer_LookupFileIsNotGeneratedMessage, Strings.InformationMessage_Title, MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                IsLoading = true;
                await _lookupService.CreateLookupFile(DataConstants.LookupDataFileName);
                IsLoading = false;

                MessageBox.Show(Strings.TlkViewer_LookupFileWasGeneratedMessage, Strings.InformationMessage_Title);
            }
        }

        IsLoading = true;
        await LoadLocalisedEntries();
        await LoadOriginalEntries();
        SelectedEntry = Entries[0];

        _saveFileDialog = new SaveFileDialog
        {
            AddExtension = true,
            AddToRecent = false,
            CheckPathExists = true,
            CreateTestFile = true,
            DereferenceLinks = true,
            Filter = DataConstants.JsonFilesFilter,
            InitialDirectory = Environment.CurrentDirectory,
            OverwritePrompt = true,
        };

        IsLoading = false;
    }

    private void FilterEntries()
    {
        Entries.Clear();
        if (string.IsNullOrWhiteSpace(Filter))
        {
            Array.ForEach(_unfilteredEntries, Entries.Add);
            SelectedEntry = _previousSelectedEntry;
            return;
        }

        foreach (var entry in _unfilteredEntries)
        {
            if (entry.Value.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            {
                Entries.Add(entry);
                if (entry == _previousSelectedEntry)
                {
                    SelectedEntry = entry;
                }
            }
        }
    }

    private void ShowEntryEditor()
    {
        var parameters = new EntryEditorParameters
        {
            StrRef = SelectedEntry.StrRef,
            OriginalValue = _originalEntries[SelectedEntry.StrRef],
            LocalisedValue = SelectedEntry.Value,
            LanguageCode = _appSettings.LanguageCode,
        };

        var entryEditorViewModel = ServiceProviderContainer.GetRequiredService<EntryEditorViewModel>();
        entryEditorViewModel.SetParameters(parameters);
        Dialog.ShowDialog(entryEditorViewModel, this);
        if (entryEditorViewModel.AreChangesSaved)
        {
            SelectedEntry.Value = entryEditorViewModel.LocalisedValue;
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                FilterEntries();
            }
        }
    }

    private void ShowContextSelector()
    {
        var contextEntriesDictionary = _unfilteredEntries
            .Where(e => e.IsContextAvailable && e.FileNames.Any(x => SelectedEntry.FileNames.Contains(x)))
            .ToDictionary(x => x.StrRef, x => x.Value);

        var parameters = new ContextSelectorParameters { StrRef = SelectedEntry.StrRef, FileNames = SelectedEntry.FileNames, TlkEntriesDictionary = contextEntriesDictionary };
        var contextSelectorViewModel = ServiceProviderContainer.GetRequiredService<ContextSelectorViewModel>();
        contextSelectorViewModel.SetParameters(parameters);
        Dialog.ShowDialog(contextSelectorViewModel, this);
    }

    private async Task ShowSettingsEditorAndReloadEntries()
    {
        var previousLocalisedTlkFilePath = _appSettings.LocalisedTlkFilePath;
        var previousOriginalTlkFilePath = _appSettings.OriginalTlkFilePath;
        var previousExtractedGameFilesPath = _appSettings.ExtractedGameFilesPath;
        var areTlkFilesValid = await ShowSettingsEditor();
        if (!areTlkFilesValid)
        {
            Entries.Clear();
            _unfilteredEntries = null;
            _originalEntries = null;
            return;
        }

        var isLookupFileGeneratedForNewPath = false;
        if (previousExtractedGameFilesPath != _appSettings.ExtractedGameFilesPath && Directory.Exists(_appSettings.ExtractedGameFilesPath))
        {
            var messageBoxResult = MessageBox.Show(Strings.TlkViewer_ExtractedGameFilesPathWasChangedMessage, Strings.InformationMessage_Title, MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                IsLoading = true;
                await _lookupService.CreateLookupFile(DataConstants.LookupDataFileName);
                isLookupFileGeneratedForNewPath = true;
            }
        }

        if (previousLocalisedTlkFilePath != _appSettings.LocalisedTlkFilePath || _unfilteredEntries == null || isLookupFileGeneratedForNewPath)
        {
            IsLoading = true;
            await LoadLocalisedEntries();
        }

        if (previousOriginalTlkFilePath != _appSettings.OriginalTlkFilePath || _originalEntries == null)
        {
            IsLoading = true;
            await LoadOriginalEntries();
        }

        IsLoading = false;
    }

    private async Task GenerateLookupFile()
    {
        if (!Directory.Exists(_appSettings.ExtractedGameFilesPath))
        {
            MessageBox.Show(Strings.TlkViewer_ExtractedGameFilesPathIsInvalidMessage, Strings.ErrorMessage_Title);
            return;
        }

        var messageBoxResult = MessageBox.Show(Strings.TlkViewer_LookupFileGenerationConfirmationMessage, Strings.InformationMessage_Title, MessageBoxButton.YesNo);
        if (messageBoxResult != MessageBoxResult.Yes)
        {
            return;
        }

        IsLoading = true;
        await _lookupService.CreateLookupFile(DataConstants.LookupDataFileName);
        await LoadLocalisedEntries();
        IsLoading = false;

        MessageBox.Show(Strings.TlkViewer_LookupFileWasGeneratedMessage, Strings.InformationMessage_Title);
    }

    private async Task ExportEntries(string[] entries)
    {
        var isFileSelected = _saveFileDialog.ShowDialog();
        if (isFileSelected != true)
        {
            return;
        }

        IsLoading = true;
        await _jsonWriter.Write(entries, _saveFileDialog.FileName);
        IsLoading = false;

        _saveFileDialog.FileName = null;
        MessageBox.Show(Strings.TlkViewer_EntriesWereExportedMessage, Strings.InformationMessage_Title);
    }

    private async Task SaveLocalisedFile()
    {
        IsLoading = true;
        var entries = _unfilteredEntries.Select(x => x.Value).ToArray();
        await _tlkWriter.WriteEntries(entries, _appSettings.LocalisedTlkFilePath);
        IsLoading = false;

        MessageBox.Show(Strings.TlkViewer_ChangesWereSavedMessage, Strings.InformationMessage_Title);
    }

    private async Task<bool> ValidateSettings()
    {
        var isEncodingNameValid = DataConstants.AvailableEncodingNames.Contains(_appSettings.EncodingName);
        var areTlkFilesValid = await ValidateTlkFiles();
        if (!isEncodingNameValid || !_appSettings.LanguageCode.IsValidLanguageCode() || !areTlkFilesValid)
        {
            if (!areTlkFilesValid)
            {
                MessageBox.Show(Strings.TlkViewer_TlkFilePathsAreInvalidMessage, Strings.ErrorMessage_Title);
            }

            areTlkFilesValid = await ShowSettingsEditor();
            if (!areTlkFilesValid)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> ShowSettingsEditor()
    {
        var settingsEditorViewModel = ServiceProviderContainer.GetRequiredService<SettingsEditorViewModel>();
        Dialog.ShowDialog(settingsEditorViewModel, this);
        var areTlkFilesValid = await ValidateTlkFiles();
        if (!areTlkFilesValid)
        {
            MessageBox.Show(Strings.TlkViewer_TlkFilePathsAreInvalidMessage, Strings.ErrorMessage_Title);
            return false;
        }

        return true;
    }

    private async Task LoadLocalisedEntries()
    {
        var lookupDictionary = File.Exists(DataConstants.LookupDataFileName)
            ? await _jsonReader.Read<Dictionary<int, string[]>>(DataConstants.LookupDataFileName)
            : new Dictionary<int, string[]>();

        Entries.Clear();
        var entries = await _tlkReader.ReadEntries(_appSettings.LocalisedTlkFilePath);
        for (var i = 0; i < entries.Length; i++)
        {
            lookupDictionary.TryGetValue(i, out var fileNames);
            var entryModel = new TlkEntryModel { Value = entries[i], StrRef = i, FileNames = fileNames };
            Entries.Add(entryModel);
        }

        _unfilteredEntries = Entries.ToArray();
    }

    private async Task LoadOriginalEntries() => _originalEntries = await _tlkReader.ReadEntries(_appSettings.OriginalTlkFilePath);

    private async Task<bool> ValidateTlkFiles()
    {
        var isLocalisedFileValid = await _tlkReader.IsValidFile(_appSettings.LocalisedTlkFilePath);
        if (!isLocalisedFileValid)
        {
            return false;
        }

        var isOriginalFileValid = await _tlkReader.IsValidFile(_appSettings.OriginalTlkFilePath);
        return isOriginalFileValid;
    }
}
