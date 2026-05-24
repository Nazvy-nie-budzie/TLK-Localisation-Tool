using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Services.Interfaces;

namespace TlkLocalisationTool.Logic.Services;

internal class JsonWriter : IJsonWriter
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public async Task Write<T>(T data, string filePath) => await Task.Run(() => WriteInternal(data, filePath));

    public void WriteSync<T>(T data, string filePath) => WriteInternal(data, filePath);

    private static void WriteInternal<T>(T data, string filePath)
    {
        var dataJson = JsonSerializer.Serialize(data, JsonSerializerOptions);
        File.WriteAllText(filePath, dataJson);
    }
}
