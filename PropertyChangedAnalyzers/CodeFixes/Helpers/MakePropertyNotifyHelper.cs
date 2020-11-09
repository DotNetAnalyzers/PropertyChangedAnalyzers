namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MakePropertyNotifyHelper
    {
        internal static PropertyDeclarationSyntax WithoutInitializer(this PropertyDeclarationSyntax property)
        {
            if (property.Initializer is null)
            {
                return property;
            }

            return property.WithInitializer(null)
                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                           .WithTrailingTrivia(property.SemicolonToken.TrailingTrivia);
        }
    }
}
