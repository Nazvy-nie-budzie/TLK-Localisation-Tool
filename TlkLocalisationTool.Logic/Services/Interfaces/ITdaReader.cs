using System.Threading.Tasks;
using TlkLocalisationTool.Shared.Entities;

namespace TlkLocalisationTool.Logic.Services.Interfaces;

public interface ITdaReader
{
    Task<TdaData> ReadData(string filePath);

    Task<int[]> ReadStrRefs(string filePath);
}
