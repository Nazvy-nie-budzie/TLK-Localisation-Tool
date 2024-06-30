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

public class DlgViewerViewModel : ViewModelBase
{
    private readonly AppSettings _appSettings;
    private readonly IGffReader _gffReader;

    private FileViewerParameters _parameters;

    private DlgEntryModel _selectedEntry;

    private Command _changeSelectedEntryCommand;

    public DlgViewerViewModel(AppSettings appSettings, IGffReader gffReader)
    {
        _appSettings = appSettings;
        _gffReader = gffReader;
    }

    public ObservableCollection<DlgEntryModel> Entries { get; } = [];

    public DlgEntryModel SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            _selectedEntry = value;
            OnPropertyChanged();
        }
    }

    public Command ChangeSelectedEntryCommand => _changeSelectedEntryCommand ??= new Command(ChangeSelectedEntry);

    public void SetParameters(FileViewerParameters parameters) => _parameters = parameters;

    public override async Task Init()
    {
        Title = string.Format(Strings.DlgViewer_Title, _parameters.FileName);

        var filePath = Path.Combine(_appSettings.ExtractedGameFilesPath, _parameters.FileName);
        var gffData = await _gffReader.ReadData(filePath);
        var startingEntries = DlgDataParser.Parse(gffData.TopLevelStruct, _parameters.TlkEntriesDictionary);
        foreach (var startingEntry in startingEntries)
        {
            Entries.Add(startingEntry);
        }
    }

    private void ChangeSelectedEntry(object newSelectedEntry) => SelectedEntry = (DlgEntryModel)newSelectedEntry;
}
