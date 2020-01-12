namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct BackingMemberMutation
    {
        internal readonly ExpressionSyntax Member;

        internal readonly IdentifierNameSyntax Parameter;

        internal BackingMemberMutation(ExpressionSyntax member, IdentifierNameSyntax parameter)
        {
            this.Member = member;
            this.Parameter = parameter;
        }
    }
}
