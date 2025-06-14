using System;
using System.Collections.Generic;
using System.Linq;
using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.Shared.Entities;

namespace TlkLocalisationTool.UI.Utils;

internal static class TdaDataParser
{
    public static TableDataSet Parse(TdaData data, Dictionary<int, string> tlkEntriesDictionary)
    {
        var originalColumnCount = data.ColumnNames.Length;
        var tableDataSet = new TableDataSet { ColumnNames = new string[originalColumnCount + 1], Rows = new TableRow[data.RowNames.Length] };
        tableDataSet.ColumnNames[0] = string.Empty;
        var strRefColumnStatuses = new bool[originalColumnCount];
        for (var i = 0; i < originalColumnCount; i++)
        {
            strRefColumnStatuses[i] = SharedFileConstants.TdaStrRefColumnNameParts.Any(x => data.ColumnNames[i].Contains(x, StringComparison.OrdinalIgnoreCase));
            tableDataSet.ColumnNames[i + 1] = data.ColumnNames[i];
        }

        var entryIndex = 0;
        for (var rowIndex = 0; rowIndex < data.RowNames.Length; rowIndex++)
        {
            var tableRow = new TableRow { Entries = new string[originalColumnCount + 1] };
            tableRow.Entries[0] = data.RowNames[rowIndex];
            for (var columnIndex = 0; columnIndex < originalColumnCount; columnIndex++)
            {
                var entry = data.Entries[entryIndex];
                tableRow.Entries[columnIndex + 1] = strRefColumnStatuses[columnIndex] ? GetStrRefEntry(entry, tlkEntriesDictionary) : entry;
                entryIndex++;
            }

            tableDataSet.Rows[rowIndex] = tableRow;
        }

        return tableDataSet;
    }

    private static string GetStrRefEntry(string entry, Dictionary<int, string> tlkEntriesDictionary)
    {
        var isEntryValidStrRef = int.TryParse(entry, out int strRef);
        if (isEntryValidStrRef)
        {
            tlkEntriesDictionary.TryGetValue(strRef, out string tlkEntry);
            return $"{entry} ({tlkEntry})";
        }

        return entry;
    }
}
