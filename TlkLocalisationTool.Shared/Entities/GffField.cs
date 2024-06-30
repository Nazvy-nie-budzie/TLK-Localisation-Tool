using TlkLocalisationTool.Shared.Enums;

namespace TlkLocalisationTool.Shared.Entities;

public class GffField
{
    public GffFieldType Type { get; set; }

    public string Label { get; set; }

    public object Data { get; set; }
}
