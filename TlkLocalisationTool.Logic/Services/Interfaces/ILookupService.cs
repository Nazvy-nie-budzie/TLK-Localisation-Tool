using System.Threading.Tasks;

namespace TlkLocalisationTool.Logic.Services.Interfaces;

public interface ILookupService
{
    Task CreateLookupFile(string filePath);
}
