using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.UI.Resources;

namespace TlkLocalisationTool.UI.Models;

public class DlgEntryModel : ModelBase
{
    private bool _isExpanded;
    private bool _isSelected;

    public string DisplayText => GetDisplayText();

    public int StrRef { get; set; }

    public string Text { get; set; }

    public string Comment { get; set; }

    public string Listener { get; set; }

    public string Speaker { get; set; }

    public DlgEntryModel[] Entries { get; set; }

    public bool IsLink { get; set; }

    public bool IsPlayerReply { get; set; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    private string GetDisplayText()
    {
        if (StrRef == SharedFileConstants.InvalidStrRef)
        {
            return Entries.Length == 0 ? Strings.DlgViewer_EndDialogueDisplayText : Strings.DlgViewer_NoTextDisplayText;
        }

        string speaker;
        if (IsPlayerReply)
        {
            speaker = Strings.DlgViewer_PlayerSpeaker;
        }
        else
        {
            speaker = string.IsNullOrEmpty(Speaker) ? Strings.DlgViewer_OwnerSpeaker : Speaker;
        }

        var displayText = $"[{speaker}] - ({StrRef}) {Text}";
        if (IsLink)
        {
            displayText = Strings.DlgViewer_LinkTag + displayText;
        }

        return displayText;
    }
}
