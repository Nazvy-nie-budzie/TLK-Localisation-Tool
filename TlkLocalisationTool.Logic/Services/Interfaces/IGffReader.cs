using System.Threading.Tasks;
using TlkLocalisationTool.Shared.Entities;

namespace TlkLocalisationTool.Logic.Services.Interfaces;

public interface IGffReader
{
    Task<GffData> ReadData(string filePath);

    Task<int[]> ReadStrRefs(string filePath);
}
