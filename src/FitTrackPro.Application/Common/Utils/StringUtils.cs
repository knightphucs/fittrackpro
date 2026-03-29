using System.Text;
using System.Text.RegularExpressions;

namespace FitTrackPro.Application.Common.Utils;

public static class StringUtils
{
    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        
        return result.Replace("đ", "d").Replace("Đ", "D");
    }
}