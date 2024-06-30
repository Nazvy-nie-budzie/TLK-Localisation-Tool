using System.Collections.Generic;
using System.Linq;
using TlkLocalisationTool.Shared.Entities;
using TlkLocalisationTool.Shared.Enums;
using TlkLocalisationTool.UI.Constants;
using TlkLocalisationTool.UI.Models;

namespace TlkLocalisationTool.UI.Utils;

internal static class DlgDataParser
{
    public static DlgEntryModel[] Parse(GffStruct topLevelStruct, Dictionary<int, string> tlkEntriesDictionary)
    {
        var npcEntryStructs = GetStructsFromFieldWithOneOfLabels(topLevelStruct.Fields, [DlgFileConstants.NpcEntryListFieldLabel]);
        var npcEntries = new DlgEntryModel[npcEntryStructs.Length];
        for (var i = 0; i < npcEntryStructs.Length; i++)
        {
            npcEntries[i] = GetEntry(npcEntryStructs[i].Fields, tlkEntriesDictionary);
        }

        var playerReplyStructs = GetStructsFromFieldWithOneOfLabels(topLevelStruct.Fields, [DlgFileConstants.PlayerReplyListFieldLabel]);
        var playerReplies = new DlgEntryModel[playerReplyStructs.Length];
        for (var i = 0; i < playerReplyStructs.Length; i++)
        {
            playerReplies[i] = GetEntry(playerReplyStructs[i].Fields, tlkEntriesDictionary);
            playerReplies[i].IsPlayerReply = true;
        }

        for (var i = 0; i < npcEntryStructs.Length; i++)
        {
            npcEntries[i].Entries = GetSubEntries(npcEntryStructs[i].Fields, playerReplies);
        }

        for (var i = 0; i < playerReplyStructs.Length; i++)
        {
            playerReplies[i].Entries = GetSubEntries(playerReplyStructs[i].Fields, npcEntries);
        }

        var startingNpcEntryStructs = GetStructsFromFieldWithOneOfLabels(topLevelStruct.Fields, [DlgFileConstants.StartingEntriesFieldLabel]);
        var startingNpcEntries = new DlgEntryModel[startingNpcEntryStructs.Length];
        for (var i = 0; i < startingNpcEntryStructs.Length; i++)
        {
            var npcEntryIndex = GetDwordFromEntryLinkFields(startingNpcEntryStructs[i].Fields);
            startingNpcEntries[i] = npcEntries[npcEntryIndex];
        }

        return startingNpcEntries;
    }

    private static DlgEntryModel GetEntry(GffField[] entryStructFields, Dictionary<int, string> tlkEntriesDictionary)
    {
        var strRef = GetStrRefFromEntryStructFields(entryStructFields);
        tlkEntriesDictionary.TryGetValue(strRef, out string text);
        var entry = new DlgEntryModel
        {
            StrRef = strRef,
            Text = text,
            Comment = GetStringFromFieldWithLabel(entryStructFields, DlgFileConstants.CommentFieldLabel),
            Listener = GetStringFromFieldWithLabel(entryStructFields, DlgFileConstants.ListenerFieldLabel),
            Speaker = GetStringFromFieldWithLabel(entryStructFields, DlgFileConstants.SpeakerFieldLabel)
        };

        return entry;
    }

    private static DlgEntryModel[] GetSubEntries(GffField[] entryStructFields, DlgEntryModel[] interlocutorEntries)
    {
        var entryLinkStructs = GetStructsFromFieldWithOneOfLabels(entryStructFields, DlgFileConstants.EntryLinksFieldLabels);
        var subEntries = new DlgEntryModel[entryLinkStructs.Length];
        for (var i = 0; i < entryLinkStructs.Length; i++)
        {
            subEntries[i] = GetSubEntry(entryLinkStructs[i].Fields, interlocutorEntries);
        }

        return subEntries;
    }

    private static DlgEntryModel GetSubEntry(GffField[] entryLinkFields, DlgEntryModel[] interlocutorEntries)
    {
        var interlocutorEntryIndex = GetDwordFromEntryLinkFields(entryLinkFields);
        var linkedInterlocutorEntry = interlocutorEntries[interlocutorEntryIndex];
        var isChild = GetBoolFromEntryLinkFields(entryLinkFields);
        if (isChild)
        {
            var subEntry = new DlgEntryModel
            {
                StrRef = linkedInterlocutorEntry.StrRef,
                Text = linkedInterlocutorEntry.Text,
                Comment = GetStringFromFieldWithLabel(entryLinkFields, DlgFileConstants.LinkCommentFieldLabel),
                Listener = linkedInterlocutorEntry.Listener,
                Speaker = linkedInterlocutorEntry.Speaker,
                IsChild = true,
                IsPlayerReply = linkedInterlocutorEntry.IsPlayerReply,
            };

            return subEntry;
        }
        else
        {
            return linkedInterlocutorEntry;
        }
    }

    private static GffStruct[] GetStructsFromFieldWithOneOfLabels(GffField[] fields, string[] fieldLabels)
    {
        var field = fields.First(x => fieldLabels.Contains(x.Label));
        return (GffStruct[])field.Data;
    }

    private static int GetStrRefFromEntryStructFields(GffField[] fields)
    {
        var field = fields.First(x => x.Type == GffFieldType.ExoLocString);
        return ((ExoLocString)field.Data).StrRef;
    }

    private static int GetDwordFromEntryLinkFields(GffField[] fields)
    {
        var field = fields.First(x => x.Type == GffFieldType.Dword);
        return (int)(uint)field.Data;
    }

    private static bool GetBoolFromEntryLinkFields(GffField[] fields)
    {
        var field = fields.FirstOrDefault(x => x.Type == GffFieldType.Byte);
        return (((byte?)field?.Data) ?? 0) > 0;
    }

    private static string GetStringFromFieldWithLabel(GffField[] fields, string fieldLabel)
    {
        var field = fields.FirstOrDefault(x => x.Label == fieldLabel);
        return (string)field?.Data;
    }
}
