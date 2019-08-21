namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameofFix))]
    [Shared]
    internal class UseNameofFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.INPC013UseNameof.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
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
                editor.SemanticModel.UnderscoreFields() != CodeStyleResult.Yes)
            {
                editor.ReplaceNode(
                    argument.Expression,
                    (x, _) => SyntaxNodeExtensions.WithTriviaFrom(SyntaxFactory.ParseExpression($"nameof(this.{name})"), x));
            }
            else
            {
                editor.ReplaceNode(
                    argument.Expression,
                    (x, _) => SyntaxNodeExtensions.WithTriviaFrom(SyntaxFactory.ParseExpression($"nameof({name})"), x));
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
