using System.Threading.Tasks;

namespace TlkLocalisationTool.Logic.Services.Interfaces;

public interface ITlkWriter
{
    Task WriteEntries(string[] entries, string filePath);
}
