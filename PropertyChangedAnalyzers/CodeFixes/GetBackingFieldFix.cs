namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GetBackingFieldFix))]
    [Shared]
    internal class GetBackingFieldFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            INPC019GetBackingField.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out AccessorDeclarationSyntax accessor) &&
                    accessor.TryFirstAncestor(out PropertyDeclarationSyntax propertyDeclaration) &&
                    Property.TryGetBackingFieldFromSetter(propertyDeclaration, semanticModel, context.CancellationToken, out var field))
                {
                    if (accessor.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
                    {
                        context.RegisterCodeFix(
                            "Get backing field.",
                            (e, cancellationToken) => e.ReplaceNode(
                                expressionBody.Expression,
                                x => Replacement(x)),
                            "Get backing field.",
                            diagnostic);
                    }
                    else if (accessor.Body is BlockSyntax block &&
                            block.Statements.TrySingle(out var statement) &&
                            statement is ReturnStatementSyntax returnStatement)
                    {
                        context.RegisterCodeFix(
                            "Get backing field.",
                            (e, cancellationToken) => e.ReplaceNode(
                                returnStatement.Expression,
                                x => Replacement(x)),
                            "Get backing field.",
                            diagnostic);
                    }

                    ExpressionSyntax Replacement(ExpressionSyntax expressionSyntax)
                    {
                        return semanticModel.UnderscoreFields()
                            ? SyntaxFactory.ParseExpression(field.Name).WithTriviaFrom(expressionSyntax)
                            : SyntaxFactory.ParseExpression($"this.{field.Name}").WithTriviaFrom(expressionSyntax);
                    }
                }
            }
        }
    }
}
