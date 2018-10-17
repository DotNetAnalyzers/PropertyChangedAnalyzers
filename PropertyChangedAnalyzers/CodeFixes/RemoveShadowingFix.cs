namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveShadowingFix))]
    [Shared]
    internal class RemoveShadowingFix : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC011DontShadow.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var eventFieldDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                      .FirstAncestorOrSelf<EventFieldDeclarationSyntax>();
                if (eventFieldDeclaration != null)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Remove shadowing event.",
                            cancellationToken => RemoveEventAsync(context.Document, eventFieldDeclaration, cancellationToken),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }

        private static async Task<Document> RemoveEventAsync(Document document, EventFieldDeclarationSyntax eventFieldDeclaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            editor.RemoveNode(eventFieldDeclaration);

            return editor.GetChangedDocument();
        }
    }
}
