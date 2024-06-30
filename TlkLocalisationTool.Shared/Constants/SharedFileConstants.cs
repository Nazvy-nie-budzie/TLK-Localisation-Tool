namespace TlkLocalisationTool.Shared.Constants;

public static class SharedFileConstants
{
    public const int InvalidStrRef = -1;

    public const string TdaFileExtension = ".2da";

    public const string DlgFileExtension = ".dlg";

    public static readonly string[] GffFileExtensions =
        [".are", ".ifo", ".bic", ".git", ".uti", ".utc", DlgFileExtension, ".itp", ".utt", ".uts", ".gff", ".fac", ".ute", ".utd", ".utp", ".gic", ".gui", ".utm", ".jrl", ".utw", ".ptm", ".ptt", ".btc", ".bti"];

    public static readonly string[] GffStrRefFieldLabelParts = ["strref"];

    public static readonly string[] TdaStrRefColumnNameParts = ["description", "string", "name", "strref", "spelldesc", "hint", "message"];
}
