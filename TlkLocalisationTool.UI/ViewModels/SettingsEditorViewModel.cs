using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TlkLocalisationTool.Logic.Services.Interfaces;
using TlkLocalisationTool.Shared.Settings;
using TlkLocalisationTool.UI.Constants;
using TlkLocalisationTool.UI.Resources;
using TlkLocalisationTool.UI.Utils;

namespace TlkLocalisationTool.UI.ViewModels;

public class SettingsEditorViewModel : ViewModelBase
{
    private readonly AppSettings _appSettings;
    private readonly ITlkReader _tlkReader;
    private readonly IJsonWriter _jsonWriter;

    private OpenFileDialog _openFileDialog;
    private OpenFolderDialog _openFolderDialog;

    private string _localisedTlkFilePath;
    private string _originalTlkFilePath;
    private string _extractedGameFilesPath;
    private string _selectedEncodingName;

    private Command _selectLocalisedTlkFilePathCommand;
    private Command _selectOriginalTlkFilePathCommand;
    private Command _selectExtractedGameFilesPathCommand;
    private Command _saveCommand;

    public SettingsEditorViewModel(AppSettings appSettings, ITlkReader tlkReader, IJsonWriter jsonWriter)
    {
        _appSettings = appSettings;
        _tlkReader = tlkReader;
        _jsonWriter = jsonWriter;
    }

    public string LocalisedTlkFilePath
    {
        get => _localisedTlkFilePath;
        set
        {
            _localisedTlkFilePath = value;
            OnPropertyChanged();
        }
    }

    public string OriginalTlkFilePath
    {
        get => _originalTlkFilePath;
        set
        {
            _originalTlkFilePath = value;
            OnPropertyChanged();
        }
    }

    public string ExtractedGameFilesPath
    {
        get => _extractedGameFilesPath;
        set
        {
            _extractedGameFilesPath = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> EncodingNames { get; } = [];

    public string SelectedEncodingName
    {
        get => _selectedEncodingName;
        set
        {
            _selectedEncodingName = value;
            OnPropertyChanged();
        }
    }

    public Command SelectLocalisedTlkFilePathCommand => _selectLocalisedTlkFilePathCommand ??= new Command(async x => LocalisedTlkFilePath = await SelectTlkFilePath(LocalisedTlkFilePath));

    public Command SelectOriginalTlkFilePathCommand => _selectOriginalTlkFilePathCommand ??= new Command(async x => OriginalTlkFilePath = await SelectTlkFilePath(OriginalTlkFilePath));

    public Command SelectExtractedGameFilesPathCommand => _selectExtractedGameFilesPathCommand ??= new Command(x => ExtractedGameFilesPath = SelectFolderPath(ExtractedGameFilesPath));

    public Command SaveCommand => _saveCommand ??= new Command(async x => await SaveAppSettings(), x => !string.IsNullOrWhiteSpace(LocalisedTlkFilePath) && !string.IsNullOrWhiteSpace(OriginalTlkFilePath));

    public override Task Init()
    {
        Title = Strings.SettingsEditor_Title;

        _openFileDialog = new OpenFileDialog
        {
            AddToRecent = false,
            CheckFileExists = true,
            CheckPathExists = true,
            DereferenceLinks = true,
            ValidateNames = true,
            Filter = DataConstants.TlkFilesFilter,
        };

        _openFolderDialog = new OpenFolderDialog
        {
            DereferenceLinks = true,
            ValidateNames = true,
        };

        LocalisedTlkFilePath = _appSettings.LocalisedTlkFilePath;
        OriginalTlkFilePath = _appSettings.OriginalTlkFilePath;
        ExtractedGameFilesPath = _appSettings.ExtractedGameFilesPath;
        Array.ForEach(DataConstants.AvailableEncodingNames, EncodingNames.Add);

        var isSelectedEncodingValid = DataConstants.AvailableEncodingNames.Contains(_appSettings.EncodingName);
        if (!isSelectedEncodingValid)
        {
            SelectedEncodingName = DataConstants.AvailableEncodingNames[0];
            if (!string.IsNullOrEmpty(_appSettings.EncodingName))
            {
                MessageBox.Show(Strings.SettingsEditor_InvalidEncodingNameMessage, Strings.ErrorMessage_Title);
            }
        }
        else
        {
            SelectedEncodingName = _appSettings.EncodingName;
        }

        return Task.CompletedTask;
    }

    private async Task<string> SelectTlkFilePath(string currentFilePath)
    {
        _openFileDialog.InitialDirectory = string.IsNullOrWhiteSpace(currentFilePath) ? Environment.CurrentDirectory : Path.GetDirectoryName(currentFilePath);        
        var dialogResult = _openFileDialog.ShowDialog();
        if (dialogResult != true)
        {
            return currentFilePath;
        }

        var isValidTlkFile = await _tlkReader.IsValidFile(_openFileDialog.FileName);
        if (isValidTlkFile)
        {
            return _openFileDialog.FileName;
        }

        MessageBox.Show(Strings.SettingsEditor_SelectedFileIsNotTlkMessage, Strings.ErrorMessage_Title);
        return currentFilePath;
    }

    private string SelectFolderPath(string currentPath)
    {
        _openFolderDialog.InitialDirectory = string.IsNullOrWhiteSpace(currentPath) ? Environment.CurrentDirectory : currentPath;
        var dialogResult = _openFolderDialog.ShowDialog();
        if (dialogResult != true)
        {
            return currentPath;
        }

        return _openFolderDialog.FolderName;
    }

    private async Task SaveAppSettings()
    {
        _appSettings.LocalisedTlkFilePath = LocalisedTlkFilePath;
        _appSettings.OriginalTlkFilePath = OriginalTlkFilePath;
        _appSettings.ExtractedGameFilesPath = ExtractedGameFilesPath;
        _appSettings.EncodingName = SelectedEncodingName;
        await _jsonWriter.Write(_appSettings, DataConstants.AppSettingsFileName);
        Close();
    }
}
