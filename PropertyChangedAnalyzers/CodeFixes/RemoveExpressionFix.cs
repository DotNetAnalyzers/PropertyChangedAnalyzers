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
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.INPC012DoNotUseExpression.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot is { } &&
                    semanticModel is { } &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ArgumentSyntax? argument) &&
                    FindName(argument) is { } nameExpression &&
                    argument.Parent is ArgumentListSyntax { Arguments: { Count: 1 }, Parent: InvocationExpressionSyntax invocation } &&
                    argument.TryFirstAncestor(out ClassDeclarationSyntax? classDeclaration) &&
                    semanticModel.TryGetNamedType(classDeclaration, context.CancellationToken, out var type) &&
                    OnPropertyChanged.Find(type, semanticModel, context.CancellationToken) is { } invoker &&
                    invoker.Parameters.TrySingle(out var parameter) &&
                    parameter.Type == KnownSymbol.String &&
                    PropertyChanged.FindPropertyName(invocation, semanticModel, context.CancellationToken) is { Value: var propertyName })
                {
                    if (parameter.IsCallerMemberName() &&
                        argument.TryFirstAncestor(out PropertyDeclarationSyntax? propertyDeclaration) &&
                        propertyDeclaration.Identifier.ValueText == propertyName)
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

        private static ExpressionSyntax? FindName(ArgumentSyntax argument)
        {
            return argument.Expression switch
            {
                ParenthesizedLambdaExpressionSyntax { Body: IdentifierNameSyntax name } => name,
                ParenthesizedLambdaExpressionSyntax { Body: MemberAccessExpressionSyntax memberAccess } => memberAccess,
                _ => null,
            };
        }
    }
}
