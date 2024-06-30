namespace TlkLocalisationTool.Logic.Constants;

internal static class TlkFileConstants
{
    public const string FileType = "TLK ";

    public const int HeaderSize = 20;

    public const int StringDataElementSize = 40;

    /// <summary>
    /// FileVersion and LanguageId combined size
    /// </summary>
    public const int HeaderStartWithoutFileTypeSize = 8;

    /// <summary>
    /// Flags, SoundResRef, VolumeVariance and PitchVariance combined size
    /// </summary>
    public const int StringDataElementStartSize = 28;

    /// <summary>
    /// SoundLength size
    /// </summary>
    public const int StringDataElementEndSize = 4;
}
