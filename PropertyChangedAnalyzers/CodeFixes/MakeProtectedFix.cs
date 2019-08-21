namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeProtectedFix))]
    [Shared]
    internal class MakeProtectedFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC018InvokerShouldBeProtected.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start) is SyntaxToken token)
                {
                    if (token.IsKind(SyntaxKind.PrivateKeyword))
                    {
                        context.RegisterCodeFix(
                            $"Change to: protected.",
                            (editor, _) => editor.ReplaceToken(
                                token,
                                SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)),
                            nameof(MakeProtectedFix),
                            diagnostic);
                    }
                    else if (token.IsKind(SyntaxKind.IdentifierToken) &&
                             token.Parent is MethodDeclarationSyntax methodDeclaration)
                    {
                        context.RegisterCodeFix(
                            $"Make protected.",
                            (editor, _) => editor.ReplaceNode(
                                methodDeclaration,
                                methodDeclaration.WithModifiers(methodDeclaration.Modifiers.Insert(0, SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)))),
                            nameof(MakeProtectedFix),
                            diagnostic);
                    }
                }
            }
        }
    }
}
