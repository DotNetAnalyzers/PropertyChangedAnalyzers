namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeProtectedFix))]
    [Shared]
    internal class MakeProtectedFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            INPC018InvokerShouldBeProtected.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start) is SyntaxToken token &&
                    token.IsKind(SyntaxKind.PrivateKeyword))
                {
                    context.RegisterCodeFix(
                        $"Change to: protected.",
                        (editor, _) => editor.ReplaceToken(
                            token,
                            SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)),
                        nameof(MakeProtectedFix),
                        diagnostic);
                }
            }
        }
    }
}
