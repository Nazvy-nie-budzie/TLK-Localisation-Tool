using System.Threading.Tasks;

namespace TlkLocalisationTool.Logic.Services.Interfaces;

public interface IJsonWriter
{
    Task Write<T>(T data, string filePath);

    void WriteSync<T>(T data, string filePath);
}
