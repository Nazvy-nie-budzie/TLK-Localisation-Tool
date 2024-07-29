using System.Collections.Generic;

namespace TlkLocalisationTool.UI.Parameters;

public class FileViewerParameters
{
    public int InitialStrRef { get; set; }

    public string FileName { get; set; }

    public Dictionary<int, string> TlkEntriesDictionary { get; set; }
}
