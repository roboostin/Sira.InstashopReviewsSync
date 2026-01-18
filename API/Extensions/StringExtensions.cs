using System.Text.RegularExpressions;

namespace API.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex EmojiRegex = new(@"\p{Cs}|" +
                                                        @"[\u203C-\u3299]|" +
                                                        @"[\uD800-\uDBFF][\uDC00-\uDFFF]|" +
                                                        @"[\u2190-\u21FF\u2300-\u23FF\u2600-\u27BF\u1F000-\u1FFFF]",
                                                        RegexOptions.Compiled);

        private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", RegexOptions.Compiled);

        private static readonly Regex NumberRegex = new(@"^[0-9()+]+$", RegexOptions.Compiled);

        static string emojiPattern = @"\p{So}|\p{Emoji_Presentation}|\p{Extended_Pictographic}|[\u{1F300}-\u{1F5FF}]|[\u{1F600}-\u{1F64F}]|[\u{1F680}-\u{1F6FF}]|[\u{2600}-\u{26FF}]|[\u{2700}-\u{27BF}]";


        public static bool IsValidWithoutEmoji(this string input) =>
            !EmojiRegex.IsMatch(input);

        public static bool ContainsEmoji(this string input) =>
            EmojiRegex.IsMatch(input);

        public static bool IsNotValidEmail(this string email) =>
            string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email);

        public static bool ContainsOnlyNumbersAndSymbols(this string input) =>
        !string.IsNullOrWhiteSpace(input) && NumberRegex.IsMatch(input);


        public static List<long> ToIdsList(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return [];

            var ids = input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(long.Parse)
                    .ToList();

            return ids;
        }

        public static List<string> ToStringList(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return [];

            var ids = input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

            return ids;
        }

    }
}
