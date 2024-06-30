using System.IO;
using System.Text;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Constants;
using TlkLocalisationTool.Logic.Services.Interfaces;
using TlkLocalisationTool.Shared.Settings;

namespace TlkLocalisationTool.Logic.Services;

internal class TlkWriter : ITlkWriter
{
    private readonly AppSettings _appSettings;

    public TlkWriter(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public async Task WriteEntries(string[] entries, string filePath) => await Task.Run(() => WriteEntriesInternal(entries, filePath));

    private void WriteEntriesInternal(string[] entries, string filePath)
    {
        var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Write);
        using var writer = new BinaryWriter(fileStream);

        writer.BaseStream.Position += TlkFileConstants.HeaderSize;

        var entryTableOffset = TlkFileConstants.StringDataElementSize * entries.Length + TlkFileConstants.HeaderSize;
        var entryOffset = 0;
        for (var i = 0; i < entries.Length; i++)
        {
            WriteEntry(writer, entries[i], entryTableOffset, entryOffset);
            entryOffset += entries[i].Length;
        }
    }

    private void WriteEntry(BinaryWriter writer, string entry, int entryTableOffset, int entryOffset)
    {
        writer.BaseStream.Position += TlkFileConstants.StringDataElementStartSize;

        writer.Write(entryOffset);
        writer.Write(entry.Length);

        var finalStreamPosition = writer.BaseStream.Position + TlkFileConstants.StringDataElementEndSize;
        writer.BaseStream.Position = entryTableOffset + entryOffset;
        WriteString(writer, entry);
        writer.BaseStream.Position = finalStreamPosition;
    }

    private void WriteString(BinaryWriter writer, string value)
    {
        var stringBytes = Encoding.GetEncoding(_appSettings.EncodingName).GetBytes(value);
        writer.Write(stringBytes);
    }
}
