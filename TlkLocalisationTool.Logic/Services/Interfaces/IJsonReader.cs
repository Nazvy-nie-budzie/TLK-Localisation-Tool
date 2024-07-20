using System.Threading.Tasks;

namespace TlkLocalisationTool.Logic.Services.Interfaces;

public interface IJsonReader
{
    Task<T> Read<T>(string filePath);

    T ReadSync<T>(string filePath);
}
