namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Text.RegularExpressions;
    using Gu.Roslyn.Asserts;

    internal static class Extensions
    {
        internal static string AssertReplaceWholeWord(this string text, string oldValue, string newValue)
        {
            var newText = text.ReplaceWholeWord(oldValue, newValue);

            if (ReferenceEquals(text, newText))
            {
                throw new AssertException($"AssertReplace failed, expected {oldValue} to be in {text}");
            }

            return newText;
        }

        internal static string ReplaceWholeWord(this string text, string oldValue, string newValue)
        {
            return Regex.Replace(text, @"\b" + Regex.Escape(oldValue) + @"\b", newValue);
        }

        internal static string EnsurePrefix(this string value, string prefix)
        {
            return !value.StartsWith(prefix, StringComparison.Ordinal)
                ? prefix + value
                : value;
        }

        internal static string RemovePrefix(this string value, string prefix)
        {
            return value.StartsWith(prefix, StringComparison.Ordinal)
                ? value.Substring(prefix.Length)
                : value;
        }
    }
}
