using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Services.Interfaces;
using TlkLocalisationTool.Shared.Settings;
using TlkLocalisationTool.UI.Models;
using TlkLocalisationTool.UI.Parameters;
using TlkLocalisationTool.UI.Resources;
using TlkLocalisationTool.UI.Utils;

namespace TlkLocalisationTool.UI.ViewModels;

public class GffViewerViewModel : ViewModelBase
{
    private readonly AppSettings _appSettings;
    private readonly IGffReader _gffReader;

    private FileViewerParameters _parameters;

    public GffViewerViewModel(AppSettings appSettings, IGffReader gffReader)
    {
        _appSettings = appSettings;
        _gffReader = gffReader;
    }

    public ObservableCollection<GffEntityModel> Entities { get; } = [];

    public void SetParameters(FileViewerParameters parameters) => _parameters = parameters;

    public override async Task Init()
    {
        Title = string.Format(Strings.GffViewer_Title, _parameters.FileName);

        var filePath = Path.Combine(_appSettings.ExtractedGameFilesPath, _parameters.FileName);
        var gffData = await _gffReader.ReadData(filePath);
        var topLevelEntity = GffDataParser.Parse(gffData.TopLevelStruct, _parameters.TlkEntriesDictionary);
        Entities.Add(topLevelEntity);
    }
}
