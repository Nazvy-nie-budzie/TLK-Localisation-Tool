using System.Text.RegularExpressions;

namespace TlkLocalisationTool.UI.Extensions;

internal static class StringExtensions
{
    private const string LanguageCodeRegex = "^[a-z]{2,3}(?:-[a-zA-Z0-9]{1,8})?$";

    public static bool IsValidLanguageCode(this string languageCode) => languageCode == string.Empty || Regex.IsMatch(languageCode, LanguageCodeRegex);
}
