namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MakePropertyNotifyHelper
    {
        internal static PropertyDeclarationSyntax WithoutInitializer(this PropertyDeclarationSyntax property)
        {
            if (property.Initializer == null)
            {
                return property;
            }

            return property.WithInitializer(null)
                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                           .WithAccessorList(
                               property.AccessorList.WithCloseBraceToken(
                                   property.AccessorList.CloseBraceToken
                                       .WithTrailingTrivia(property.SemicolonToken.TrailingTrivia)));
        }
    }
}
