namespace TlkLocalisationTool.Shared.Entities;

public class GffStruct
{
    public uint Type { get; set; }

    public int[] FieldIndicies { get; set; }

    public GffField[] Fields { get; set; }
}
