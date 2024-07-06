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
using TlkLocalisationTool.UI.Extensions;
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
    private string _languageCode;
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

    public ObservableCollection<string> EncodingNames { get; } = [];

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

    public string LanguageCode
    {
        get => _languageCode;
        set
        {
            _languageCode = value;
            OnPropertyChanged();
        }
    }

    public string SelectedEncodingName
    {
        get => _selectedEncodingName;
        set
        {
            _selectedEncodingName = value;
            OnPropertyChanged();
        }
    }

    public Command SelectLocalisedTlkFilePathCommand => _selectLocalisedTlkFilePathCommand ??= new Command(async _ => LocalisedTlkFilePath = await SelectTlkFilePath(LocalisedTlkFilePath));

    public Command SelectOriginalTlkFilePathCommand => _selectOriginalTlkFilePathCommand ??= new Command(async _ => OriginalTlkFilePath = await SelectTlkFilePath(OriginalTlkFilePath));

    public Command SelectExtractedGameFilesPathCommand => _selectExtractedGameFilesPathCommand ??= new Command(_ => ExtractedGameFilesPath = SelectFolderPath(ExtractedGameFilesPath));

    public Command SaveCommand => _saveCommand ??= new Command(async _ => await SaveAppSettings(), _ => !string.IsNullOrWhiteSpace(LocalisedTlkFilePath) && !string.IsNullOrWhiteSpace(OriginalTlkFilePath));

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
            AddToRecent = false,
            DereferenceLinks = true,
            ValidateNames = true,
        };

        LocalisedTlkFilePath = _appSettings.LocalisedTlkFilePath;
        OriginalTlkFilePath = _appSettings.OriginalTlkFilePath;
        ExtractedGameFilesPath = _appSettings.ExtractedGameFilesPath;
        SetLanguageCode();
        Array.ForEach(DataConstants.AvailableEncodingNames, EncodingNames.Add);
        SetSelectedEncodingName();

        return Task.CompletedTask;
    }

    private async Task<string> SelectTlkFilePath(string currentFilePath)
    {
        _openFileDialog.InitialDirectory = string.IsNullOrWhiteSpace(currentFilePath) ? Environment.CurrentDirectory : Path.GetDirectoryName(currentFilePath);        
        var isFileSelected = _openFileDialog.ShowDialog();
        if (isFileSelected != true)
        {
            return currentFilePath;
        }

        var isValidTlkFile = await _tlkReader.IsValidFile(_openFileDialog.FileName);
        if (isValidTlkFile)
        {
            return _openFileDialog.FileName;
        }

        MessageBox.Show(Strings.SettingsEditor_SelectedFileIsNotValidTlkMessage, Strings.ErrorMessage_Title);
        return currentFilePath;
    }

    private string SelectFolderPath(string currentPath)
    {
        _openFolderDialog.InitialDirectory = string.IsNullOrWhiteSpace(currentPath) ? Environment.CurrentDirectory : currentPath;
        var isFolderSelected = _openFolderDialog.ShowDialog();
        if (isFolderSelected != true)
        {
            return currentPath;
        }

        return _openFolderDialog.FolderName;
    }

    private void SetLanguageCode()
    {
        if (!_appSettings.LanguageCode.IsValidLanguageCode())
        {
            MessageBox.Show(Strings.SettingsEditor_InvalidLanguageCodeWasClearedMessage, Strings.ErrorMessage_Title);
            _appSettings.LanguageCode = string.Empty;
        }

        LanguageCode = _appSettings.LanguageCode;
    }

    private void SetSelectedEncodingName()
    {
        var isSelectedEncodingValid = DataConstants.AvailableEncodingNames.Contains(_appSettings.EncodingName);
        if (!isSelectedEncodingValid)
        {
            if (!string.IsNullOrEmpty(_appSettings.EncodingName))
            {
                MessageBox.Show(Strings.SettingsEditor_InvalidEncodingNameWasReplacedMessage, Strings.ErrorMessage_Title);
            }

            _appSettings.EncodingName = DataConstants.AvailableEncodingNames[0];
        }

        SelectedEncodingName = _appSettings.EncodingName;
    }

    private async Task SaveAppSettings()
    {
        if (!LanguageCode.IsValidLanguageCode())
        {
            MessageBox.Show(Strings.SettingsEditor_InvalidLanguageCodeMessage, Strings.ErrorMessage_Title);
            return;
        }

        _appSettings.LocalisedTlkFilePath = LocalisedTlkFilePath;
        _appSettings.OriginalTlkFilePath = OriginalTlkFilePath;
        _appSettings.ExtractedGameFilesPath = ExtractedGameFilesPath;
        _appSettings.LanguageCode = LanguageCode;
        _appSettings.EncodingName = SelectedEncodingName;
        await _jsonWriter.Write(_appSettings, DataConstants.AppSettingsFileName);
        Close();
    }
}
