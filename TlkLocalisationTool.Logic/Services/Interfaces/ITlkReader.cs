using System.Threading.Tasks;

namespace TlkLocalisationTool.Logic.Services.Interfaces;

public interface ITlkReader
{
    Task<string[]> ReadEntries(string filePath);

    Task<bool> IsValidFile(string filePath);
}
