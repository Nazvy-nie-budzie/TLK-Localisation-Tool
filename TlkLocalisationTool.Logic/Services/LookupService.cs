using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Constants;
using TlkLocalisationTool.Logic.Services.Interfaces;
using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.Shared.Settings;

namespace TlkLocalisationTool.Logic.Services;

internal class LookupService : ILookupService
{
    private readonly AppSettings _appSettings;
    private readonly IGffReader _gffReader;
    private readonly ITdaReader _tdaReader;
    private readonly ISsfReader _ssfReader;
    private readonly IJsonWriter _jsonWriter;

    public LookupService(AppSettings appSettings, IGffReader gffReader, ITdaReader tdaReader, ISsfReader ssfReader, IJsonWriter jsonWriter)
    {
        _appSettings = appSettings;
        _gffReader = gffReader;
        _tdaReader = tdaReader;
        _ssfReader = ssfReader;
        _jsonWriter = jsonWriter;
    }

    public async Task CreateLookupFile(string filePath) => await Task.Run(() => CreateLookupFileInternal(filePath));

    private async Task CreateLookupFileInternal(string filePath)
    {
        var lookupDictionary = new Dictionary<int, List<string>>();
        var gameFilePaths = Directory.GetFiles(_appSettings.ExtractedGameFilesPath, string.Empty, SearchOption.AllDirectories);
        foreach (var gameFilePath in gameFilePaths)
        {
            Func<string, Task<int[]>> strRefsReader = null;
            var fileExtension = Path.GetExtension(gameFilePath);
            if (SharedFileConstants.GffFileExtensions.Contains(fileExtension))
            {
                strRefsReader = _gffReader.ReadStrRefs;
            }
            else if (fileExtension == SharedFileConstants.TdaFileExtension)
            {
                strRefsReader = _tdaReader.ReadStrRefs;
            }
            else if (fileExtension == SsfFileConstants.FileExtension)
            {
                strRefsReader = _ssfReader.ReadStrRefs;
            }

            if (strRefsReader != null)
            {
                await AddStrRefsToLookupDictionary(gameFilePath, strRefsReader, lookupDictionary);
            }
        }

        var sortedLookupDictionary = lookupDictionary.OrderBy(x => x.Key).ToDictionary();
        await _jsonWriter.Write(sortedLookupDictionary, filePath);
    }

    private async Task AddStrRefsToLookupDictionary(string filePath, Func<string, Task<int[]>> strRefsReader, Dictionary<int, List<string>> lookupDictionary)
    {
        var filePathForLookup = GetFilePathForLookup(filePath);
        var strRefs = await strRefsReader(filePath);
        foreach (var strRef in strRefs)
        {
            if (!lookupDictionary.TryGetValue(strRef, out var strRefFilePaths))
            {
                strRefFilePaths = new List<string>();
                lookupDictionary.Add(strRef, strRefFilePaths);
            }

            strRefFilePaths.Add(filePathForLookup);
        }
    }

    private string GetFilePathForLookup(string filePath) => Path.GetRelativePath(_appSettings.ExtractedGameFilesPath, filePath).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
