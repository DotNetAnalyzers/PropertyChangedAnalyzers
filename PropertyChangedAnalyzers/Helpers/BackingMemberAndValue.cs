namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct BackingMemberAndValue
    {
        internal readonly ExpressionSyntax Member;

        internal readonly IdentifierNameSyntax Parameter;

        internal BackingMemberAndValue(ExpressionSyntax member, IdentifierNameSyntax parameter)
        {
            this.Member = member;
            this.Parameter = parameter;
        }
    }
}
