using System.Collections.Generic;

namespace TlkLocalisationTool.UI.Parameters;

public class FileViewerParameters
{
    public string FileName { get; set; }

    public Dictionary<int, string> TlkEntriesDictionary { get; set; }
}
