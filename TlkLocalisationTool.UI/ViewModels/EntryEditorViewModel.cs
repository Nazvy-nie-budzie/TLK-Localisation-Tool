using System.Threading.Tasks;
using TlkLocalisationTool.UI.Parameters;
using TlkLocalisationTool.UI.Resources;
using TlkLocalisationTool.UI.Utils;

namespace TlkLocalisationTool.UI.ViewModels;

public class EntryEditorViewModel : ViewModelBase
{
    private int _strRef;

    private Command _saveCommand;

    public bool AreChangesSaved { get; private set; }

    public string OriginalValue { get; set; }

    public string LocalisedValue { get; set; }

    public Command SaveCommand => _saveCommand ??= new Command(x => SaveChanges());

    public void SetParameters(EntryEditorParameters parameters)
    {
        _strRef = parameters.StrRef;
        OriginalValue = parameters.OriginalValue;
        LocalisedValue = parameters.LocalisedValue;
    }

    public override Task Init()
    {
        Title = string.Format(Strings.EntryEditor_Title, _strRef);
        return Task.CompletedTask;
    }

    private void SaveChanges()
    {
        AreChangesSaved = true;
        Close();
    }
}
