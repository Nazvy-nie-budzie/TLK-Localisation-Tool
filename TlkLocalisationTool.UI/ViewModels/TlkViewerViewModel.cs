using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
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

    private string[] _originalEntries;
    private TlkEntryModel _previousSelectedEntry;
    private OpenFileDialog _openFileDialog;
    private SaveFileDialog _saveFileDialog;
    private bool _isFilterByOriginalEntries;
    private TlkEntryModel _selectedEntry;

    private Command _filterCommand;
    private Command _editCommand;
    private Command _selectContextCommand;
    private Command _settingsCommand;
    private Command _generateLookupFileCommand;
    private Command _importLocalisedCommand;
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

    public bool IsFilterByOriginalEntries
    {
        get => _isFilterByOriginalEntries;
        set
        {
            _isFilterByOriginalEntries = value;
            FilterEntries();
        }
    }

    public TlkEntryModel SelectedEntry
    {
        get => _selectedEntry;
        set => UpdateSelectedEntry(value);
    }

    public Command FilterCommand => _filterCommand ??= new Command(_ => FilterEntries());

    public Command EditCommand => _editCommand ??= new Command(_ => ShowEntryEditor(), _ => SelectedEntry != null);

    public Command SelectContextCommand => _selectContextCommand ??= new Command(_ => ShowContextSelector(), _ => SelectedEntry?.IsContextAvailable == true);

    public Command SettingsCommand => _settingsCommand ??= new Command(async _ => await ShowSettingsEditorAndReloadEntries());

    public Command GenerateLookupFileCommand => _generateLookupFileCommand ??= new Command(async _ => await GenerateLookupFile());

    public Command ImportLocalisedCommand => _importLocalisedCommand ??= new Command(async _ => await ImportEntries(), _ => Entries != null);

    public Command ExportLocalisedCommand => _exportLocalisedCommand ??= new Command(
        async _ => await ExportEntries(Entries.Select(x => x.Value).ToArray()),
        _ => Entries != null);

    public Command ExportOriginalCommand => _exportOriginalCommand ??= new Command(async _ => await ExportEntries(_originalEntries), _ => _originalEntries != null);

    public Command SaveCommand => _saveCommand ??= new Command(async _ => await SaveLocalisedFile(), _ => Entries != null);

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
            var messageBoxResult = MessageBox.Show(Strings.TlkViewer_LookupFileIsNotGeneratedMessage, Strings.InformationMessage_Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                IsLoading = true;
                await _lookupService.CreateLookupFile(DataConstants.LookupDataFileName);
                IsLoading = false;

                MessageBox.Show(Strings.TlkViewer_LookupFileWasGeneratedMessage, Strings.InformationMessage_Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        IsLoading = true;
        await LoadLocalisedEntries();
        await LoadOriginalEntries();
        IsLoading = false;
    }

    private void FilterEntries()
    {
        if (SelectedEntry != null)
        {
            SelectedEntry = null;
        }

        var collectionView = CollectionViewSource.GetDefaultView(Entries);
        if (string.IsNullOrWhiteSpace(Filter))
        {
            collectionView.Filter = null;
            SelectedEntry = _previousSelectedEntry;
            return;
        }

        collectionView.Filter = IsEntryFilteredIn;
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
        var contextEntriesDictionary = Entries
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
            _originalEntries = null;
            return;
        }

        var isLookupFileGeneratedForNewPath = false;
        if (previousExtractedGameFilesPath != _appSettings.ExtractedGameFilesPath && Directory.Exists(_appSettings.ExtractedGameFilesPath))
        {
            var messageBoxResult = MessageBox.Show(Strings.TlkViewer_ExtractedGameFilesPathWasChangedMessage, Strings.InformationMessage_Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                IsLoading = true;
                await _lookupService.CreateLookupFile(DataConstants.LookupDataFileName);
                isLookupFileGeneratedForNewPath = true;
            }
        }

        if (previousLocalisedTlkFilePath != _appSettings.LocalisedTlkFilePath || Entries == null || isLookupFileGeneratedForNewPath)
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
            MessageBox.Show(Strings.TlkViewer_ExtractedGameFilesPathIsInvalidMessage, Strings.ErrorMessage_Title, MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var messageBoxResult = MessageBox.Show(Strings.TlkViewer_LookupFileGenerationConfirmationMessage, Strings.InformationMessage_Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (messageBoxResult != MessageBoxResult.Yes)
        {
            return;
        }

        IsLoading = true;
        await _lookupService.CreateLookupFile(DataConstants.LookupDataFileName);
        await LoadLocalisedEntries();
        IsLoading = false;

        MessageBox.Show(Strings.TlkViewer_LookupFileWasGeneratedMessage, Strings.InformationMessage_Title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task ImportEntries()
    {
        var messageBoxResult = MessageBox.Show(Strings.TlkViewer_UnsavedChangesWillBeLostMessage, Strings.WarningMessage_Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (messageBoxResult != MessageBoxResult.Yes)
        {
            return;
        }

        _openFileDialog ??= new OpenFileDialog
        {
            AddExtension = true,
            AddToRecent = false,
            CheckPathExists = true,
            DereferenceLinks = true,
            Filter = DataConstants.JsonFilesFilter,
            InitialDirectory = Environment.CurrentDirectory,
            ValidateNames = true,
        };

        var isFileSelected = _openFileDialog.ShowDialog();
        if (isFileSelected != true)
        {
            return;
        }

        IsLoading = true;
        var importedEntries = await _jsonReader.Read<string[]>(_openFileDialog.FileName);
        if (importedEntries.Length != Entries.Count)
        {
            MessageBox.Show(Strings.TlkViewer_ImportedEntriesCountIsNotEqualToOriginalEntriesCount, Strings.ErrorMessage_Title, MessageBoxButton.OK, MessageBoxImage.Error);
            IsLoading = false;
            return;
        }

        for (var i = 0; i < importedEntries.Length; i++)
        {
            Entries[i].Value = importedEntries[i];
        }

        IsLoading = false;

        _openFileDialog.FileName = null;
        MessageBox.Show(Strings.TlkViewer_EntriesWereImportedMessage, Strings.InformationMessage_Title, MessageBoxButton.OK, MessageBoxImage.Information);
        FilterEntries();
    }

    private async Task ExportEntries(string[] entries)
    {
        _saveFileDialog ??= new SaveFileDialog
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

        var isFileSelected = _saveFileDialog.ShowDialog();
        if (isFileSelected != true)
        {
            return;
        }

        IsLoading = true;
        await _jsonWriter.Write(entries, _saveFileDialog.FileName);
        IsLoading = false;

        _saveFileDialog.FileName = null;
        MessageBox.Show(Strings.TlkViewer_EntriesWereExportedMessage, Strings.InformationMessage_Title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task SaveLocalisedFile()
    {
        IsLoading = true;
        var entries = Entries.Select(x => x.Value).ToArray();
        await _tlkWriter.WriteEntries(entries, _appSettings.LocalisedTlkFilePath);
        IsLoading = false;

        MessageBox.Show(Strings.TlkViewer_ChangesWereSavedMessage, Strings.InformationMessage_Title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task<bool> ValidateSettings()
    {
        IsLoading = true;
        var isEncodingNameValid = DataConstants.AvailableEncodingNames.Contains(_appSettings.EncodingName);
        var areTlkFilesValid = await ValidateTlkFiles();
        IsLoading = false;

        if (!isEncodingNameValid || !_appSettings.LanguageCode.IsValidLanguageCode() || !areTlkFilesValid)
        {
            if (!areTlkFilesValid)
            {
                MessageBox.Show(Strings.TlkViewer_TlkFilePathsAreInvalidMessage, Strings.ErrorMessage_Title, MessageBoxButton.OK, MessageBoxImage.Error);
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

        IsLoading = true;
        var areTlkFilesValid = await ValidateTlkFiles();
        IsLoading = false;

        if (!areTlkFilesValid)
        {
            MessageBox.Show(Strings.TlkViewer_TlkFilePathsAreInvalidMessage, Strings.ErrorMessage_Title, MessageBoxButton.OK, MessageBoxImage.Error);
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

        SelectInitialEntry();
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

    private void SelectInitialEntry()
    {
        if (_appSettings.LastSelectedStrRef > Entries.Count - 1 || _appSettings.LastSelectedStrRef < 0)
        {
            _appSettings.LastSelectedStrRef = 0;
        }

        SelectedEntry = Entries[_appSettings.LastSelectedStrRef];
    }

    private void UpdateSelectedEntry(TlkEntryModel newSelectedEntry)
    {
        if (newSelectedEntry != null)
        {
            _appSettings.LastSelectedStrRef = newSelectedEntry.StrRef;
        }

        _previousSelectedEntry = _selectedEntry;
        _selectedEntry = newSelectedEntry;
        OnPropertyChanged(nameof(SelectedEntry));
    }

    private bool IsEntryFilteredIn(object item)
    {
        var entry = item as TlkEntryModel;
        var isEntryFiltered = entry.Value.Contains(Filter, StringComparison.OrdinalIgnoreCase)
            || (IsFilterByOriginalEntries && _originalEntries[entry.StrRef].Contains(Filter, StringComparison.OrdinalIgnoreCase));

        if (isEntryFiltered && entry == _previousSelectedEntry)
        {
            SelectedEntry = entry;
        }

        return isEntryFiltered;
    }
}
