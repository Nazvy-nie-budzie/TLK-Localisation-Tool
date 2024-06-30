using System;
using System.Collections.Generic;
using System.Linq;
using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.Shared.Entities;
using TlkLocalisationTool.UI.Models;
using TlkLocalisationTool.UI.Resources;

namespace TlkLocalisationTool.UI.Utils;

internal static class TdaDataParser
{
    public static TdaColumnModel[] Parse(TdaData data, Dictionary<int, string> tlkEntriesDictionary)
    {
        var columnCount = data.ColumnNames.Length;
        var rowCount = data.RowNames.Length;
        var columns = new TdaColumnModel[columnCount + 1];
        columns[0] = new TdaColumnModel { Name = Strings.TdaViewer_RowNamesColumnName, Values = data.RowNames };
        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            var columnName = data.ColumnNames[columnIndex];
            var isStrRefColumn = SharedFileConstants.TdaStrRefColumnNameParts.Any(x => columnName.Contains(x, StringComparison.OrdinalIgnoreCase));
            var columnValues = new string[rowCount];
            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var entryIndex = rowIndex * columnCount + columnIndex;
                var entry = data.Entries[entryIndex];
                columnValues[rowIndex] = isStrRefColumn ? GetStrRefEntry(entry, tlkEntriesDictionary) : entry;
            }

            columns[columnIndex + 1] = new TdaColumnModel { Name = columnName, Values = columnValues };
        }

        return columns;
    }

    private static string GetStrRefEntry(string entry, Dictionary<int, string> tlkEntriesDictionary)
    {
        var isEntryValidStrRef = int.TryParse(entry, out int strRef);
        if (isEntryValidStrRef)
        {
            tlkEntriesDictionary.TryGetValue(strRef, out string value);
            return $"{entry} ({value})";
        }

        return entry;
    }
}
