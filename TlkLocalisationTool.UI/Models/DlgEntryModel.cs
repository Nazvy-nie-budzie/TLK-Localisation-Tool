using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.UI.Resources;

namespace TlkLocalisationTool.UI.Models;

public class DlgEntryModel
{
    public int StrRef { get; set; }

    public string Text { get; set; }

    public string Comment { get; set; }

    public string Listener { get; set; }

    public string Speaker { get; set; }

    public DlgEntryModel[] Entries { get; set; }

    public bool IsChild { get; set; }

    public bool IsPlayerReply { get; set; }

    public string DisplayText => GetDisplayText();

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
        if (IsChild)
        {
            displayText = Strings.DlgViewer_LinkTag + displayText;
        }

        return displayText;
    }
}
