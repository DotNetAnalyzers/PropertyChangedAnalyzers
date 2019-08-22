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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveExpressionFix))]
    [Shared]
    internal class RemoveExpressionFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.INPC012DoNotUseExpression.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ArgumentSyntax argument) &&
                    TryGetNameExpression(argument, out var nameExpression) &&
                    argument.Parent is ArgumentListSyntax argumentList &&
                    argumentList.Arguments.Count == 1 &&
                    argumentList.Parent is InvocationExpressionSyntax invocation &&
                    argument.TryFirstAncestor(out ClassDeclarationSyntax classDeclaration) &&
                    semanticModel.TryGetSymbol(classDeclaration, context.CancellationToken, out var type) &&
                    OnPropertyChanged.TryFind(type, semanticModel, context.CancellationToken, out var invoker) &&
                    invoker.Parameters.TrySingle(out var parameter) &&
                    parameter.Type == KnownSymbol.String &&
                    PropertyChanged.TryGetName(invocation, semanticModel, context.CancellationToken, out var name) == AnalysisResult.Yes)
                {
                    if (parameter.IsCallerMemberName() &&
                        argument.TryFirstAncestor(out PropertyDeclarationSyntax propertyDeclaration) &&
                        propertyDeclaration.Identifier.ValueText == name)
                    {
                        context.RegisterCodeFix(
                            "Use overload that does not use expression.",
                            (editor, cancellationToken) => editor.ReplaceNode(
                                invocation,
                                x => x.WithArgumentList(SyntaxFactory.ArgumentList())
                                      .WithTriviaFrom(x)),
                            nameof(RemoveExpressionFix),
                            diagnostic);
                    }
                    else
                    {
                        context.RegisterCodeFix(
                            "Use overload that does not use expression.",
                            (editor, cancellationToken) => editor.ReplaceNode(
                                argument.Expression,
                                x => InpcFactory.Nameof(nameExpression)),
                            nameof(RemoveExpressionFix),
                            diagnostic);
                    }
                }
            }
        }

        private static bool TryGetNameExpression(ArgumentSyntax argument, out ExpressionSyntax expression)
        {
            if (argument.Expression is ParenthesizedLambdaExpressionSyntax lambda)
            {
                switch (lambda.Body)
                {
                    case IdentifierNameSyntax identifierName:
                        expression = identifierName;
                        return true;
                    case MemberAccessExpressionSyntax memberAccess:
                        expression = memberAccess;
                        return true;
                }
            }

            expression = null;
            return false;
        }
    }
}
