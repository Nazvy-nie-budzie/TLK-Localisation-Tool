using System.Threading.Tasks;

namespace TlkLocalisationTool.Logic.Services.Interfaces;

public interface ISsfReader
{
    Task<int[]> ReadStrRefs(string filePath);
}
