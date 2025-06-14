using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.Shared.Settings;
using TlkLocalisationTool.UI.Models;
using TlkLocalisationTool.UI.Parameters;
using TlkLocalisationTool.UI.Resources;
using TlkLocalisationTool.UI.Utils;

namespace TlkLocalisationTool.UI.ViewModels;

public class ContextSelectorViewModel : ViewModelBase
{
    private readonly AppSettings _appSettings;

    private ContextSelectorParameters _parameters;

    private Command _showContextCommand;

    public ContextSelectorViewModel(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public ObservableCollection<FileNameModel> FileNames { get; } = [];

    public FileNameModel SelectedFileName { get; set; }

    public Command ShowContextCommand => _showContextCommand ??= new Command(_ => ShowContext(), _ => SelectedFileName?.IsContextAvailable == true);

    public void SetParameters(ContextSelectorParameters parameters) => _parameters = parameters;

    public override Task Init()
    {
        Title = string.Format(Strings.ContextSelector_Title, _parameters.StrRef);

        IsLoading = true;
        var areAllFilesExist = true;
        foreach (var fileName in _parameters.FileNames)
        {
            var filePath = Path.Combine(_appSettings.ExtractedGameFilesPath, fileName);
            var isFileExist = File.Exists(filePath);
            areAllFilesExist &= isFileExist;
            var fileExtension = Path.GetExtension(fileName);
            var isContextAvailableForFileExtension = fileExtension == SharedFileConstants.TdaFileExtension || SharedFileConstants.GffFileExtensions.Contains(fileExtension);
            var fileNameModel = new FileNameModel { Value = fileName, IsContextAvailable = isFileExist && isContextAvailableForFileExtension };
            FileNames.Add(fileNameModel);
        }

        IsLoading = false;

        if (!areAllFilesExist)
        {
            MessageBox.Show(Strings.ContextSelectror_SomeFilesDontExistMessage, Strings.ErrorMessage_Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return Task.CompletedTask;
    }

    private void ShowContext()
    {
        var fileExtension = Path.GetExtension(SelectedFileName.Value);
        if (fileExtension == SharedFileConstants.TdaFileExtension)
        {
            ShowTdaViewer();
        }
        else if (fileExtension == SharedFileConstants.DlgFileExtension)
        {
            ShowDlgViewer();
        }
        else
        {
            ShowGffViewer();
        }
    }

    private void ShowTdaViewer()
    {
        var tdaViewerViewModel = ServiceProviderContainer.GetRequiredService<TdaViewerViewModel>();
        tdaViewerViewModel.SetParameters(GetFileViewerParameters());
        Dialog.ShowDialog(tdaViewerViewModel, this);
    }

    private void ShowDlgViewer()
    {
        var dlgViewerViewModel = ServiceProviderContainer.GetRequiredService<DlgViewerViewModel>();
        dlgViewerViewModel.SetParameters(GetFileViewerParameters());
        Dialog.ShowDialog(dlgViewerViewModel, this);
    }

    private void ShowGffViewer()
    {
        var gffViewerViewModel = ServiceProviderContainer.GetRequiredService<GffViewerViewModel>();
        gffViewerViewModel.SetParameters(GetFileViewerParameters());
        Dialog.ShowDialog(gffViewerViewModel, this);
    }

    private FileViewerParameters GetFileViewerParameters()
    {
        var parameters = new FileViewerParameters { InitialStrRef = _parameters.StrRef, FileName = SelectedFileName.Value, TlkEntriesDictionary = _parameters.TlkEntriesDictionary };
        return parameters;
    }
}
