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
    private SaveFileDialog _saveFileDialog;

    private Command _filterCommand;
    private Command _editCommand;
    private Command _selectContextCommand;
    private Command _settingsCommand;
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

    public TlkEntryModel SelectedEntry { get; set; }

    public Command FilterCommand => _filterCommand ??= new Command(_ => FilterEntries());

    public Command EditCommand => _editCommand ??= new Command(_ => ShowEntryEditor(), _ => SelectedEntry != null);

    public Command SelectContextCommand => _selectContextCommand ??= new Command(_ => ShowContextSelector(), _ => SelectedEntry?.IsContextAvailable == true);

    public Command SettingsCommand => _settingsCommand ??= new Command(async _ => await ShowSettingsEditorAndReloadEntries());

    public Command ExportLocalisedCommand => _exportLocalisedCommand ??= new Command(
        async _ => await ExportEntries(_unfilteredEntries.Select(x => x.Value).ToArray()),
        _ => _unfilteredEntries != null);

    public Command ExportOriginalCommand => _exportOriginalCommand ??= new Command(async _ => await ExportEntries(_originalEntries), _ => _originalEntries != null);

    public Command SaveCommand => _saveCommand ??= new Command(async _ => await SaveLocalisedFile(), _ => _unfilteredEntries != null);

    public override async Task Init()
    {
        Title = Strings.TlkViewer_Title;

        var isEncodingNameValid = DataConstants.AvailableEncodingNames.Contains(_appSettings.EncodingName);
        if (!isEncodingNameValid || !_appSettings.LanguageCode.IsValidLanguageCode() || !AreFilePathsSet())
        {
            var areFilePathsSet = ShowSettingsEditor();
            if (!areFilePathsSet)
            {
                return;
            }
        }

        IsLoading = true;
        if (!string.IsNullOrWhiteSpace(_appSettings.ExtractedGameFilesPath) && !File.Exists(DataConstants.LookupDataFileName))
        {
            var messageBoxResult = MessageBox.Show(Strings.TlkViewer_LookupFileIsNotGeneratedMessage, Strings.InformationMessage_Title, MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                await _lookupService.CreateLookupFile(DataConstants.LookupDataFileName);
                MessageBox.Show(Strings.TlkViewer_LookupFileWasGeneratedMessage, Strings.InformationMessage_Title);
            }
        }

        await LoadLocalisedEntries();
        await LoadOriginalEntries();

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
            return;
        }

        foreach (var entry in _unfilteredEntries)
        {
            if (entry.Value.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            {
                Entries.Add(entry);
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
        }
    }

    private void ShowContextSelector()
    {
        var contextEntriesDictionary = _unfilteredEntries
            .Where(e => e.IsContextAvailable && e.FilePaths.Any(x => SelectedEntry.FilePaths.Contains(x)))
            .ToDictionary(x => x.StrRef, x => x.Value);

        var parameters = new ContextSelectorParameters { StrRef = SelectedEntry.StrRef, FilePaths = SelectedEntry.FilePaths, TlkEntriesDictionary = contextEntriesDictionary };
        var contextSelectorViewModel = ServiceProviderContainer.GetRequiredService<ContextSelectorViewModel>();
        contextSelectorViewModel.SetParameters(parameters);
        Dialog.ShowDialog(contextSelectorViewModel, this);
    }

    private async Task ShowSettingsEditorAndReloadEntries()
    {
        var previousLocalisedTlkFilePath = _appSettings.LocalisedTlkFilePath;
        var previousOriginalTlkFilePath = _appSettings.OriginalTlkFilePath;
        var areFilePathsSet = ShowSettingsEditor();
        if (!areFilePathsSet)
        {
            return;
        }

        IsLoading = true;
        if (previousLocalisedTlkFilePath != _appSettings.LocalisedTlkFilePath)
        {
            await LoadLocalisedEntries();
        }

        if (previousOriginalTlkFilePath != _appSettings.OriginalTlkFilePath)
        {
            await LoadOriginalEntries();
        }

        IsLoading = false;
    }

    private async Task ExportEntries(string[] entries)
    {
        var isFileSelected = _saveFileDialog.ShowDialog();
        if (isFileSelected != true)
        {
            return;
        }

        await _jsonWriter.Write(entries, _saveFileDialog.FileName);
        _saveFileDialog.FileName = null;
        MessageBox.Show(Strings.TlkViewer_EntriesWereExportedMessage, Strings.InformationMessage_Title);
    }

    private async Task SaveLocalisedFile()
    {
        var entries = _unfilteredEntries.Select(x => x.Value).ToArray();
        await _tlkWriter.WriteEntries(entries, _appSettings.LocalisedTlkFilePath);
    }

    private bool ShowSettingsEditor()
    {
        var settingsEditorViewModel = ServiceProviderContainer.GetRequiredService<SettingsEditorViewModel>();
        Dialog.ShowDialog(settingsEditorViewModel, this);
        if (!AreFilePathsSet())
        {
            MessageBox.Show(Strings.TlkViewer_TlkFilePathsAreNotSetUpMessage, Strings.ErrorMessage_Title);
            return false;
        }

        return true;
    }

    private async Task LoadLocalisedEntries()
    {
        var lookupDictionary = await _jsonReader.Read<Dictionary<int, string[]>>(DataConstants.LookupDataFileName);
        var entries = await _tlkReader.ReadEntries(_appSettings.LocalisedTlkFilePath);
        for (var i = 0; i < entries.Length; i++)
        {
            lookupDictionary.TryGetValue(i, out var filePaths);
            var entryModel = new TlkEntryModel { Value = entries[i], StrRef = i, FilePaths = filePaths };
            Entries.Add(entryModel);
        }

        _unfilteredEntries = Entries.ToArray();
    }

    private async Task LoadOriginalEntries() => _originalEntries = await _tlkReader.ReadEntries(_appSettings.OriginalTlkFilePath);

    private bool AreFilePathsSet() => !string.IsNullOrWhiteSpace(_appSettings.LocalisedTlkFilePath) && !string.IsNullOrWhiteSpace(_appSettings.OriginalTlkFilePath);
}
