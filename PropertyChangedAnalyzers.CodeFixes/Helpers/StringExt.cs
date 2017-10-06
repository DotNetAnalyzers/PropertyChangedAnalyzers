namespace PropertyChangedAnalyzers
{
    internal static class StringExt
    {
        internal static string ToFirstCharLower(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var chars = text.ToCharArray();
            chars[0] = char.ToLowerInvariant(chars[0]);
            return new string(chars);
        }
    }
}