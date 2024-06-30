namespace TlkLocalisationTool.Shared.Enums;

public enum GffFieldType
{
    Byte = 0,

    Char = 1,

    Word = 2,

    Short = 3,

    Dword = 4,

    Int = 5,

    Dword64 = 6,

    Int64 = 7,

    Float = 8,

    Double = 9,

    ExoString = 10,

    ResRef = 11,

    ExoLocString = 12,

    Void = 13,

    Struct = 14,

    List = 15,

    Rotation = 16,

    Vector = 17,

    /// <summary>
    /// Doesn't actually exist in GFF files, here just to show it on UI
    /// </summary>
    ExoLocSubString = 100,
}
