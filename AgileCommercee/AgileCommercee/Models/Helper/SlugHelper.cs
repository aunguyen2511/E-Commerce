using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace AgileCommercee.Models.Helper
{
    public static class SlugHelper
    {
        public static string ToSlug(this string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            title = title.ToLowerInvariant();

            // Thay "đ" thành "d"
            title = title.Replace("đ", "d");

            // Loại bỏ dấu bằng cách sử dụng Normalization
            var normalizedTitle = title.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedTitle)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            title = stringBuilder.ToString();

            // Loại bỏ các ký tự không phải chữ cái, số và thay khoảng trắng thành dấu '-'
            title = Regex.Replace(title, @"[^a-z0-9\s-]", "");
            title = Regex.Replace(title, @"\s+", "-").Trim('-');

            // Loại bỏ dấu '-' trùng lặp
            title = Regex.Replace(title, @"-+", "-");
            return title;

        }

    }
}
