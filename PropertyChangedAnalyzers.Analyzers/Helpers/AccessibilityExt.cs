namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class AccessibilityExt
    {
        internal static bool IsEither(this Accessibility accessibility, Accessibility accessibility1, Accessibility accessibility2)
        {
            return accessibility == accessibility1 || accessibility == accessibility2;
        }
    }
}