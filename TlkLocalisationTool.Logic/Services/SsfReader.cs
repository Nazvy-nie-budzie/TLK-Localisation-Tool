using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Constants;
using TlkLocalisationTool.Logic.Extensions;
using TlkLocalisationTool.Logic.Services.Interfaces;
using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.Shared.Settings;

namespace TlkLocalisationTool.Logic.Services;

internal class SsfReader : ISsfReader
{
    private readonly AppSettings _appSettings;

    public SsfReader(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public async Task<int[]> ReadStrRefs(string filePath) => await Task.Run(() => ReadStrRefsInternal(filePath));

    public int[] ReadStrRefsInternal(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));
        CheckFileType(reader, filePath);

        reader.BaseStream.Position = FileConstants.FileVersionSize + SsfFileConstants.HeaderEndSize;

        var strRefs = new List<int>();
        var entryCount = (reader.BaseStream.Length - reader.BaseStream.Position) / SsfFileConstants.StrRefSize;
        for (var i = 0; i < entryCount; i++)
        {
            var strRef = reader.ReadInt32();
            if (strRef != SharedFileConstants.InvalidStrRef && !strRefs.Contains(strRef))
            {
                strRefs.Add(strRef);
            }
        }

        return strRefs.ToArray();
    }

    private void CheckFileType(BinaryReader reader, string filePath)
    {
        var fileType = reader.ReadString(FileConstants.FileTypeSize, _appSettings.EncodingName);
        if (fileType != SsfFileConstants.FileType)
        {
            throw new ArgumentException($"File {filePath} is not a SSF file");
        }
    }
}
