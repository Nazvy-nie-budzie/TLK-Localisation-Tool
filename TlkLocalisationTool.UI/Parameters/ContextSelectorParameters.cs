using System.Collections.Generic;

namespace TlkLocalisationTool.UI.Parameters;

public class ContextSelectorParameters
{
    public int StrRef { get; set; }

    public string[] FileNames { get; set; }

    public Dictionary<int, string> TlkEntriesDictionary { get; set; }
}
