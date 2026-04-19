using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Common.Helpers;

public static class SlugHelper
{
    public static string ToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string str = input.ToLower().Trim();
        str = RemoveDiacritics(str);
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", "-");
        str = Regex.Replace(str, @"-+", "-");

        return str;
    }

    private static readonly Dictionary<string, string> VietnameseMap = new()
    {
        {"à","a"},{"á","a"},{"ả","a"},{"ã","a"},{"ạ","a"},
        {"ă","a"},{"ằ","a"},{"ắ","a"},{"ẳ","a"},{"ẵ","a"},{"ặ","a"},
        {"â","a"},{"ầ","a"},{"ấ","a"},{"ẩ","a"},{"ẫ","a"},{"ậ","a"},
        {"è","e"},{"é","e"},{"ẻ","e"},{"ẽ","e"},{"ẹ","e"},
        {"ê","e"},{"ề","e"},{"ế","e"},{"ể","e"},{"ễ","e"},{"ệ","e"},
        {"ì","i"},{"í","i"},{"ỉ","i"},{"ĩ","i"},{"ị","i"},
        {"ò","o"},{"ó","o"},{"ỏ","o"},{"õ","o"},{"ọ","o"},
        {"ô","o"},{"ồ","o"},{"ố","o"},{"ổ","o"},{"ỗ","o"},{"ộ","o"},
        {"ơ","o"},{"ờ","o"},{"ớ","o"},{"ở","o"},{"ỡ","o"},{"ợ","o"},
        {"ù","u"},{"ú","u"},{"ủ","u"},{"ũ","u"},{"ụ","u"},
        {"ư","u"},{"ừ","u"},{"ứ","u"},{"ử","u"},{"ữ","u"},{"ự","u"},
        {"ỳ","y"},{"ý","y"},{"ỷ","y"},{"ỹ","y"},{"ỵ","y"},
        {"đ","d"},
    };

    private static string RemoveDiacritics(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (VietnameseMap.TryGetValue(c.ToString(), out var replacement))
                sb.Append(replacement);
            else
                sb.Append(c);
        }

        var normalized = sb.ToString().Normalize(NormalizationForm.FormD);
        var result = new StringBuilder();
        foreach (var c in normalized)
        {
            if (char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                result.Append(c);
        }
        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
