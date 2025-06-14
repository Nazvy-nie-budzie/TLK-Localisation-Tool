using System.IO;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Services.Interfaces;
using TlkLocalisationTool.Shared.Settings;
using TlkLocalisationTool.UI.Parameters;
using TlkLocalisationTool.UI.Resources;
using TlkLocalisationTool.UI.Utils;

namespace TlkLocalisationTool.UI.ViewModels;

public class TdaViewerViewModel : ViewModelBase
{
    private readonly AppSettings _appSettings;
    private readonly ITdaReader _tdaReader;

    private FileViewerParameters _parameters;

    private TableDataSet _tableDataSet;

    public TdaViewerViewModel(AppSettings appSettings, ITdaReader tdaReader)
    {
        _appSettings = appSettings;
        _tdaReader = tdaReader;
    }

    public TableDataSet TableDataSet
    {
        get => _tableDataSet;
        set
        {
            _tableDataSet = value;
            OnPropertyChanged();
        }
    }

    public void SetParameters(FileViewerParameters parameters) => _parameters = parameters;

    public override async Task Init()
    {
        Title = string.Format(Strings.TdaViewer_Title, _parameters.FileName);

        IsLoading = true;
        var filePath = Path.Combine(_appSettings.ExtractedGameFilesPath, _parameters.FileName);
        var tdaData = await _tdaReader.ReadData(filePath);
        TableDataSet = TdaDataParser.Parse(tdaData, _parameters.TlkEntriesDictionary);
        IsLoading = false;
    }
}
