namespace TlkLocalisationTool.UI.Models;

public class TlkEntryModel : ModelBase
{
    private string _value;

    public bool IsContextAvailable => FilePaths != null;

    public int StrRef { get; set; }

    public string[] FilePaths { get; set; }

    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged();
        }
    }
}
