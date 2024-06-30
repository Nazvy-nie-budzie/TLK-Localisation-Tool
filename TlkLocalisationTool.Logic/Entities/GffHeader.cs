namespace TlkLocalisationTool.Logic.Entities;

internal class GffHeader
{
    public string FileType { get; set; }

    public string FileVersion { get; set; }

    public int StructBlockOffset { get; set; }

    public int StructCount { get; set; }

    public int FieldBlockOffset { get; set; }

    public int FieldCount { get; set; }

    public int LabelBlockOffset { get; set; }

    public int LabelCount { get; set; }

    public int FieldDataBlockOffset { get; set; }

    public int FieldDataCount { get; set; }

    public int FieldIndicesBlockOffset { get; set; }

    public int FieldIndicesCount { get; set; }

    public int ListIndicesBlockOffset { get; set; }

    public int ListIndicesCount { get; set; }
}
