using System;
using System.IO;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Constants;
using TlkLocalisationTool.Logic.Extensions;
using TlkLocalisationTool.Logic.Services.Interfaces;
using TlkLocalisationTool.Shared.Settings;

namespace TlkLocalisationTool.Logic.Services;

internal class TlkReader : ITlkReader
{
    private readonly AppSettings _appSettings;

    public TlkReader(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public async Task<string[]> ReadEntries(string filePath) => await Task.Run(() => ReadEntriesInternal(filePath));

    public async Task<bool> IsValidFile(string filePath) => await Task.Run(() => IsValidInternal(filePath));

    private string[] ReadEntriesInternal(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));
        if (!IsValidFileType(reader))
        {
            throw new ArgumentException($"File {filePath} is not a TLK file");
        }

        reader.BaseStream.Position += TlkFileConstants.HeaderStartWithoutFileTypeSize;

        var entryCount = reader.ReadInt32();
        var entryTableOffset = reader.ReadInt32();
        var entries = new string[entryCount];
        for (var i = 0; i < entryCount; i++)
        {
            entries[i] = ReadEntry(reader, entryTableOffset);
        }

        return entries;
    }

    private bool IsValidInternal(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        using var reader = new BinaryReader(File.OpenRead(filePath));
        return IsValidFileType(reader);
    }

    private string ReadEntry(BinaryReader reader, int entryTableOffset)
    {
        reader.BaseStream.Position += TlkFileConstants.StringDataElementStartSize;

        var entryOffset = entryTableOffset + reader.ReadInt32();
        var entrySize = reader.ReadInt32();
        var finalStreamPosition = reader.BaseStream.Position + TlkFileConstants.StringDataElementEndSize;
        reader.BaseStream.Position = entryOffset;
        var entry = reader.ReadString(entrySize, _appSettings.EncodingName);
        reader.BaseStream.Position = finalStreamPosition;

        return entry;
    }

    private bool IsValidFileType(BinaryReader reader)
    {
        var fileType = reader.ReadString(FileConstants.FileTypeSize, _appSettings.EncodingName);
        return fileType == TlkFileConstants.FileType;
    }
}
