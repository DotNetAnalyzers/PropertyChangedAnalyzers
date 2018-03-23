namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal sealed class SyntaxTokenValueTextComparer : IEqualityComparer<SyntaxToken>
    {
        public static readonly SyntaxTokenValueTextComparer Default = new SyntaxTokenValueTextComparer();

        public bool Equals(SyntaxToken x, SyntaxToken y) => x.ValueText == y.ValueText;

        public int GetHashCode(SyntaxToken obj) => obj.ValueText.GetHashCode();
    }
}
