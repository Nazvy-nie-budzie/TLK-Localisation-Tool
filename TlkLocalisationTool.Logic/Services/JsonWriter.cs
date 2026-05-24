using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Services.Interfaces;

namespace TlkLocalisationTool.Logic.Services;

internal class JsonWriter : IJsonWriter
{
    private static readonly JsonSerializerOptions MultilineJsonSerializerOptions = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private static readonly JsonSerializerOptions OneLineJsonSerializerOptions = new() { WriteIndented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public async Task Write<T>(T data, string filePath, bool isWriteIndented) => await Task.Run(() => WriteInternal(data, filePath, isWriteIndented));

    public void WriteSync<T>(T data, string filePath, bool isWriteIndented) => WriteInternal(data, filePath, isWriteIndented);

    private static void WriteInternal<T>(T data, string filePath, bool isWriteIndented)
    {
        var jsonSerializerOptions = isWriteIndented ? MultilineJsonSerializerOptions : OneLineJsonSerializerOptions;
        var dataJson = JsonSerializer.Serialize(data, jsonSerializerOptions);
        File.WriteAllText(filePath, dataJson);
    }
}
