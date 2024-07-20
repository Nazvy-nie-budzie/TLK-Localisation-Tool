using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Services.Interfaces;

namespace TlkLocalisationTool.Logic.Services;

internal class JsonReader : IJsonReader
{
    public async Task<T> Read<T>(string filePath) => await Task.Run(() => ReadInternal<T>(filePath));

    public T ReadSync<T>(string filePath) => ReadInternal<T>(filePath);

    private static T ReadInternal<T>(string filePath) => JsonSerializer.Deserialize<T>(File.ReadAllText(filePath));
}
