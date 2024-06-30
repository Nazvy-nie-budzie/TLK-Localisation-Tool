using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.UI.Models;
using TlkLocalisationTool.UI.Parameters;
using TlkLocalisationTool.UI.Resources;
using TlkLocalisationTool.UI.Utils;

namespace TlkLocalisationTool.UI.ViewModels;

public class ContextSelectorViewModel : ViewModelBase
{
    private int _strRef; 
    private Dictionary<int, string> _tlkEntriesDictionary;

    private Command _showContextCommand;

    public ObservableCollection<FilePathModel> FilePaths { get; } = [];

    public FilePathModel SelectedFilePath { get; set; }

    public Command ShowContextCommand => _showContextCommand ??= new Command(x => ShowContext(), x => SelectedFilePath?.IsContextAvailable == true);

    public void SetParameters(ContextSelectorParameters parameters)
    {
        _strRef = parameters.StrRef;
        _tlkEntriesDictionary = parameters.TlkEntriesDictionary;
        foreach (var filePath in parameters.FilePaths)
        {
            var fileExtension = Path.GetExtension(filePath);
            var isContextAvailable = fileExtension == SharedFileConstants.TdaFileExtension || SharedFileConstants.GffFileExtensions.Contains(fileExtension);
            var filePathModel = new FilePathModel { Value = filePath, IsContextAvailable = isContextAvailable };
            FilePaths.Add(filePathModel);
        }
    }

    public override Task Init()
    {
        Title = string.Format(Strings.ContextSelector_Title, _strRef);
        return Task.CompletedTask;
    }

    private void ShowContext()
    {
        var fileExtension = Path.GetExtension(SelectedFilePath.Value);
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
        var parameters = new FileViewerParameters { FileName = SelectedFilePath.Value, TlkEntriesDictionary = _tlkEntriesDictionary };
        return parameters;
    }
}
