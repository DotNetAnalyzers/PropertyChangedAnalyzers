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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CheckIfDifferentBeforeNotifyFix))]
    [Shared]
    internal class CheckIfDifferentBeforeNotifyFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.INPC005CheckIfDifferentBeforeNotifying.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                      .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ExpressionStatementSyntax onPropertyChangedStatement) &&
                    onPropertyChangedStatement.TryFirstAncestor(out AccessorDeclarationSyntax setter) &&
                    setter.IsKind(SyntaxKind.SetAccessorDeclaration) &&
                    setter.Body is BlockSyntax body)
                {
                    if (Setter.TryFindSingleAssignment(setter, out var assignment) &&
                        assignment.Parent is ExpressionStatementSyntax assignmentStatement &&
                        body.Statements.IndexOf(assignmentStatement) == 0)
                    {
                        if (semanticModel.TryGetSymbol(assignment.Left, CancellationToken.None, out var assignedSymbol) &&
                            assignedSymbol.Kind == SymbolKind.Field &&
                            semanticModel.TryGetSymbol(setter, context.CancellationToken, out IMethodSymbol setterSymbol) &&
                            TrySet.TryFind(setterSymbol.ContainingType, semanticModel, context.CancellationToken, out var trySetMethod) &&
                            TrySet.CanCreateInvocation(trySetMethod, out _) &&
                            setter.TryFirstAncestor(out PropertyDeclarationSyntax property))
                        {
                            if (setter.Body.Statements.Count == 2)
                            {
                                context.RegisterCodeFix(
                                trySetMethod.DisplaySignature(),
                                async (editor, cancellationToken) =>
                                {
                                    var trySet = await editor.TrySetInvocationAsync(trySetMethod, assignment.Left, assignment.Right, property, cancellationToken)
                                                             .ConfigureAwait(false);
                                    _ = editor.ReplaceNode(
                                          setter,
                                          x => x.AsExpressionBody(trySet));
                                },
                                trySetMethod.MetadataName,
                                diagnostic);
                            }
                        }

                        context.RegisterCodeFix(
                            "Check that value is different before notifying.",
                            (editor, cancellationToken) => editor.InsertBefore(
                                assignmentStatement,
                                InpcFactory.IfReturn(
                                    InpcFactory.Equals(
                                        assignment.Right,
                                        assignment.Left,
                                        editor.SemanticModel,
                                        cancellationToken))),
                            nameof(CheckIfDifferentBeforeNotifyFix),
                            diagnostic);
                    }
                    else if (onPropertyChangedStatement.Parent == body &&
                             Setter.TryFindSingleTrySet(setter, semanticModel, context.CancellationToken, out var trySet))
                    {
                        switch (trySet.Parent)
                        {
                            case ExpressionStatementSyntax trySetStatement
                                when body.Statements.IndexOf(trySetStatement) == body.Statements.IndexOf(onPropertyChangedStatement) - 1:
                                context.RegisterCodeFix(
                                    "Check that value is different before notifying.",
                                    (editor, __) =>
                                    {
                                        editor.RemoveNode(onPropertyChangedStatement);
                                        _ = editor.ReplaceNode(
                                            trySetStatement,
                                            x => InpcFactory.IfStatement(
                                                x.Expression.WithoutTrivia(),
                                                onPropertyChangedStatement));
                                    },
                                    nameof(CheckIfDifferentBeforeNotifyFix),
                                    diagnostic);
                                break;
                            case IfStatementSyntax ifTrySet
                                when body.Statements.IndexOf(ifTrySet) == body.Statements.IndexOf(onPropertyChangedStatement) - 1:
                                context.RegisterCodeFix(
                                    "Check that value is different before notifying.",
                                    (editor, _) => editor.MoveOnPropertyChangedInside(
                                        ifTrySet,
                                        onPropertyChangedStatement),
                                    nameof(CheckIfDifferentBeforeNotifyFix),
                                    diagnostic);
                                break;
                        }
                    }
                }
            }
        }
    }
}
