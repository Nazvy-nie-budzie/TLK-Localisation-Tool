namespace TlkLocalisationTool.UI.Models;

public class TlkEntryModel : ModelBase
{
    private string _value;

    public bool IsContextAvailable => FileNames != null;

    public int StrRef { get; set; }

    public string[] FileNames { get; set; }

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
