using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Constants;
using TlkLocalisationTool.Logic.Extensions;
using TlkLocalisationTool.Logic.Services.Interfaces;
using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.Shared.Entities;
using TlkLocalisationTool.Shared.Settings;

namespace TlkLocalisationTool.Logic.Services;

internal class TdaReader : ITdaReader
{
    private readonly AppSettings _appSettings;

    public TdaReader(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public async Task<TdaData> ReadData(string filePath) => await Task.Run(() => ReadDataInternal(filePath));

    public async Task<int[]> ReadStrRefs(string filePath) => await Task.Run(() => ReadStrRefsInternal(filePath));

    private TdaData ReadDataInternal(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));
        CheckFileType(reader, filePath);

        reader.BaseStream.Position += FileConstants.FileVersionSize + TdaFileConstants.NewLineCharSize;

        var tdaData = new TdaData { ColumnNames = ReadColumnNames(reader) };
        var rowCount = reader.ReadInt32();
        tdaData.RowNames = ReadRowNames(reader, rowCount);
        tdaData.Entries = ReadEntries(reader, rowCount, tdaData.ColumnNames.Length);
        return tdaData;
    }

    private int[] ReadStrRefsInternal(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));
        CheckFileType(reader, filePath);

        reader.BaseStream.Position += FileConstants.FileVersionSize + TdaFileConstants.NewLineCharSize;

        var columnNames = ReadColumnNames(reader);
        var strRefColumnIndicies = GetStrRefColumnIndicies(columnNames);
        if (strRefColumnIndicies.Length == 0)
        {
            return [];
        }

        var rowCount = reader.ReadInt32();
        SkipRowNames(reader, rowCount);

        var entries = ReadStrRefs(reader, rowCount, columnNames.Length, strRefColumnIndicies);
        return entries;
    }

    private void CheckFileType(BinaryReader reader, string filePath)
    {
        var fileType = reader.ReadString(FileConstants.FileTypeSize, _appSettings.EncodingName);
        if (fileType != TdaFileConstants.FileType)
        {
            throw new ArgumentException($"File {filePath} is not a 2DA file");
        }
    }

    private string[] ReadColumnNames(BinaryReader reader)
    {
        var columnNames = new List<string>();
        var currentColumnNameBytes = new List<byte>();
        var charByte = reader.ReadByte();
        while (charByte != TdaFileConstants.NullCharByte)
        {
            if (charByte == TdaFileConstants.TabCharByte)
            {
                columnNames.Add(GetString(currentColumnNameBytes.ToArray(), _appSettings.EncodingName));
                currentColumnNameBytes.Clear();
            }
            else
            {
                currentColumnNameBytes.Add(charByte);
            }

            charByte = reader.ReadByte();
        }

        return columnNames.ToArray();
    }

    private string[] ReadRowNames(BinaryReader reader, int rowCount)
    {
        var rowNames = new string[rowCount];
        var currentRowNameBytes = new List<byte>();
        var currentRowNameIndex = 0;
        while (currentRowNameIndex < rowCount)
        {
            var charByte = reader.ReadByte();
            if (charByte == TdaFileConstants.TabCharByte)
            {
                rowNames[currentRowNameIndex] = GetString(currentRowNameBytes.ToArray(), _appSettings.EncodingName);
                currentRowNameBytes.Clear();
                currentRowNameIndex++;
            }
            else
            {
                currentRowNameBytes.Add(charByte);
            }
        }

        return rowNames;
    }

    private string[] ReadEntries(BinaryReader reader, int rowCount, int columnCount)
    {
        var rowDataOffsetsStreamPosition = reader.BaseStream.Position;
        var rowDataBlockStreamPosition = reader.BaseStream.Position + GetRowDataBlockPositionOffset(rowCount, columnCount);
        var entries = new string[rowCount * columnCount];
        for (var i = 0; i < entries.Length; i++)
        {
            var entryDataOffsetStreamPosition = rowDataOffsetsStreamPosition + i * TdaFileConstants.RowDataOffsetSize;
            entries[i] = ReadEntry(reader, rowDataBlockStreamPosition, entryDataOffsetStreamPosition);
        }

        return entries;
    }

    private static int[] GetStrRefColumnIndicies(string[] columnNames)
    {
        var strRefColumnIndicies = new List<int>();
        for (var i = 0; i < columnNames.Length; i++)
        {
            if (SharedFileConstants.TdaStrRefColumnNameParts.Any(x => columnNames[i].Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                strRefColumnIndicies.Add(i);
            }
        }

        return strRefColumnIndicies.ToArray();
    }

    private static void SkipRowNames(BinaryReader reader, int rowCount)
    {
        var readRowNameCount = 0;
        while (readRowNameCount < rowCount)
        {
            if (reader.ReadByte() == TdaFileConstants.TabCharByte)
            {
                readRowNameCount++;
            }
        }
    }

    private int[] ReadStrRefs(BinaryReader reader, int rowCount, int columnCount, int[] strRefColumnIndicies)
    {
        var rowDataOffsetsStreamPosition = reader.BaseStream.Position;
        var rowDataBlockStreamPosition = reader.BaseStream.Position + GetRowDataBlockPositionOffset(rowCount, columnCount);
        var strRefs = new List<int>();
        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            var rowStartIndex = rowIndex * columnCount;
            for (var columnIndiciesIndex = 0; columnIndiciesIndex < strRefColumnIndicies.Length; columnIndiciesIndex++)
            {
                var entryIndex = rowStartIndex + strRefColumnIndicies[columnIndiciesIndex];
                var entryDataOffsetStreamPosition = rowDataOffsetsStreamPosition + entryIndex * TdaFileConstants.RowDataOffsetSize;
                var entry = ReadEntry(reader, rowDataBlockStreamPosition, entryDataOffsetStreamPosition);
                if (int.TryParse(entry, out var strRef) && strRef != SharedFileConstants.InvalidStrRef && !strRefs.Contains(strRef))
                {
                    strRefs.Add(strRef);
                }
            }
        }

        return strRefs.ToArray();
    }

    private string ReadEntry(BinaryReader reader, long rowDataBlockStreamPosition, long entryDataOffsetStreamPosition)
    {
        reader.BaseStream.Position = entryDataOffsetStreamPosition;
        var entryDataOffset = reader.ReadUInt16();
        reader.BaseStream.Position = rowDataBlockStreamPosition + entryDataOffset;
        var entry = ReadStringToNullChar(reader);
        return entry;
    }

    private string ReadStringToNullChar(BinaryReader reader)
    {
        var stringBytes = new List<byte>();
        var charByte = reader.ReadByte();
        while (charByte != TdaFileConstants.NullCharByte)
        {
            stringBytes.Add(charByte);
            charByte = reader.ReadByte();
        }

        return GetString(stringBytes.ToArray(), _appSettings.EncodingName);
    }

    private static long GetRowDataBlockPositionOffset(int rowCount, int columnCount) => rowCount * columnCount * TdaFileConstants.RowDataOffsetSize + TdaFileConstants.RowDataBlockSizeSize;

    private static string GetString(byte[] stringBytes, string encodingName) => Encoding.GetEncoding(encodingName).GetString(stringBytes);
}
