using System.IO;
using System.Text;

namespace TlkLocalisationTool.Logic.Extensions;

internal static class BinaryReaderExtensions
{
    private const string NullString = "\0";

    public static string ReadString(this BinaryReader reader, int size, string encodingName)
    {
        var stringBytes = reader.ReadBytes(size);
        var result = Encoding.GetEncoding(encodingName).GetString(stringBytes).Replace(NullString, string.Empty);
        return result;
    }
}
