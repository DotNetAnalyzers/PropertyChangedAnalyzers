namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameofCodeFixProvider))]
    [Shared]
    internal class UseNameofCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC013UseNameof.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

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
                    context.RegisterDocumentEditorFix(
                        "Use nameof",
                        (editor, cancellationToken) => ApplyFix(editor, argument, literal.Token.ValueText, cancellationToken),
                        this.GetType(),
                        diagnostic);
                }
            }
        }

        private static void ApplyFix(DocumentEditor editor, ArgumentSyntax argument, string name, CancellationToken cancellationToken)
        {
            if (!IsStaticContext(argument, editor.SemanticModel, cancellationToken) &&
                editor.SemanticModel.LookupSymbols(argument.SpanStart, name: name).TrySingle(out var member) &&
                (member is IFieldSymbol || member is IPropertySymbol || member is IMethodSymbol) &&
                !member.IsStatic &&
                !argument.UsesUnderscore(editor.SemanticModel, cancellationToken))
            {
                editor.ReplaceNode(
                    argument.Expression,
                    (x, _) => SyntaxFactory.ParseExpression($"nameof(this.{name})")
                                           .WithTriviaFrom(x));
            }
            else
            {
                editor.ReplaceNode(
                    argument.Expression,
                    (x, _) => SyntaxFactory.ParseExpression($"nameof({name})")
                                           .WithTriviaFrom(x));
            }
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