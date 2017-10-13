namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameofCodeFixProvider))]
    [Shared]
    internal class UseNameofCodeFixProvider : CodeFixProvider
    {
        private static readonly IdentifierNameSyntax NameofIdentifier = SyntaxFactory.IdentifierName(@"nameof");

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC013UseNameof.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText) || token.IsMissing)
                {
                    continue;
                }

                var argument = (ArgumentSyntax)syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
                if (argument.Expression is LiteralExpressionSyntax literal)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use nameof",
                            cancellationToken => ApplyFixAsync(context.Document, argument, literal.Token.ValueText, cancellationToken),
                            nameof(UseNameofCodeFixProvider)),
                        diagnostic);
                }
            }
        }

        private static async Task<Document> ApplyFixAsync(Document document, ArgumentSyntax argument, string name, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            if (!IsStaticContext(argument, editor.SemanticModel, cancellationToken) &&
                editor.SemanticModel.LookupSymbols(argument.SpanStart, name: name).TryGetSingle(out var member) &&
                (member is IFieldSymbol || member is IPropertySymbol || member is IMethodSymbol) &&
                !member.IsStatic &&
                !argument.UsesUnderscoreNames(editor.SemanticModel, cancellationToken))
            {
                editor.ReplaceNode(argument.Expression, SyntaxFactory.ParseExpression($"nameof(this.{name})"));
            }
            else
            {
                editor.ReplaceNode(argument.Expression, SyntaxFactory.ParseExpression($"nameof({name})"));
            }

            return editor.GetChangedDocument();
        }

        private static bool IsStaticContext(SyntaxNode context, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var accessor = context.FirstAncestor<AccessorDeclarationSyntax>();
            if (accessor != null)
            {
                return semanticModel.GetDeclaredSymbolSafe(accessor.FirstAncestor<PropertyDeclarationSyntax>(), cancellationToken)
                                    ?.IsStatic != false;
            }

            var methodDeclaration = context.FirstAncestor<MethodDeclarationSyntax>();
            return semanticModel.GetDeclaredSymbolSafe(methodDeclaration, cancellationToken)?.IsStatic != false;
        }
    }
}