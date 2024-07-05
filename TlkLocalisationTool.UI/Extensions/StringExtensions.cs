using System.Text.RegularExpressions;

namespace TlkLocalisationTool.UI.Extensions;

internal static class StringExtensions
{
    // doesn't fully cover IETF language code standard, but should be good enough, as it covers stuff like be, or be-BY
    private const string LanguageCodeRegex = "^[a-z]{2,3}(?:-[a-zA-Z0-9]{1,8})?$";

    public static bool IsValidLanguageCode(this string languageCode) => languageCode != null && (languageCode == string.Empty || Regex.IsMatch(languageCode, LanguageCodeRegex));
}
