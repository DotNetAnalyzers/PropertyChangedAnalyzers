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
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC005CheckIfDifferentBeforeNotifying.Id);

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
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ExpressionStatementSyntax? onPropertyChangedStatement) &&
                    onPropertyChangedStatement.TryFirstAncestor(out AccessorDeclarationSyntax? setter) &&
                    setter.IsKind(SyntaxKind.SetAccessorDeclaration) &&
                    setter.Body is { } body)
                {
                    if (Setter.FindSingleAssignment(setter) is { Parent: ExpressionStatementSyntax assignmentStatement } assignment &&
                        body.Statements.IndexOf(assignmentStatement) == 0)
                    {
                        if (semanticModel.TryGetSymbol(assignment.Left, CancellationToken.None, out var assignedSymbol) &&
                            assignedSymbol.Kind == SymbolKind.Field &&
                            semanticModel.TryGetSymbol(setter, context.CancellationToken, out IMethodSymbol? setterSymbol) &&
                            TrySet.Find(setterSymbol.ContainingType, semanticModel, context.CancellationToken) is { } trySetMethod &&
                            TrySet.CanCreateInvocation(trySetMethod) is { } &&
                            setter.TryFirstAncestor(out PropertyDeclarationSyntax? property))
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
                             Setter.FindSingleTrySet(setter, semanticModel, context.CancellationToken) is { } trySet)
                    {
                        switch (trySet.Parent)
                        {
                            case AssignmentExpressionSyntax { Parent: ExpressionStatementSyntax assignStatement }
                                when body.Statements.IndexOf(assignStatement) == body.Statements.IndexOf(onPropertyChangedStatement) - 1:
                                context.RegisterCodeFix(
                                    "Check that value is different before notifying.",
                                    (editor, __) =>
                                    {
                                        editor.RemoveNode(onPropertyChangedStatement);
                                        _ = editor.ReplaceNode(
                                            assignStatement,
                                            x => InpcFactory.IfStatement(
                                                ((AssignmentExpressionSyntax)x.Expression).Right.WithoutTrivia(),
                                                onPropertyChangedStatement));
                                    },
                                    nameof(CheckIfDifferentBeforeNotifyFix),
                                    diagnostic);
                                break;
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
