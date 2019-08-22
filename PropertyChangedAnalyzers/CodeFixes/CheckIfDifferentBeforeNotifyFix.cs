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
                    if (Property.TrySingleAssignmentInSetter(setter, out var assignment) &&
                        assignment.Parent is ExpressionStatementSyntax assignmentStatement &&
                        body.Statements.IndexOf(assignmentStatement) == 0)
                    {
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

                        if (setter.Body.Statements.Count == 2 &&
                            semanticModel.TryGetSymbol(assignment.Left, CancellationToken.None, out var assignedSymbol) &&
                            assignedSymbol.Kind == SymbolKind.Field &&
                            semanticModel.TryGetSymbol(setter, context.CancellationToken, out IMethodSymbol setterSymbol) &&
                            PropertyChanged.TryGetSetAndRaise(setterSymbol.ContainingType, semanticModel, context.CancellationToken, out var setAndRaiseMethod) &&
                           InpcFactory.CanGenerateSetAndRaiseCall(setAndRaiseMethod, out var nameParameter))
                        {
                            context.RegisterCodeFix(
                                $"Use {setAndRaiseMethod.ContainingType.MetadataName}.{setAndRaiseMethod.MetadataName}",
                                async (editor, cancellationToken) =>
                                {
                                    var qualifyAccess = await editor.QualifyMethodAccessAsync(cancellationToken)
                                                                    .ConfigureAwait(false);
                                    var nameExpression = await editor.NameOfContainingAsync(setter.FirstAncestorOrSelf<PropertyDeclarationSyntax>(), nameParameter, cancellationToken)
                                                                     .ConfigureAwait(false);
                                    _ = editor.ReplaceNode(
                                          setter,
                                          x => x.AsExpressionBody(InpcFactory.SetAndRaise(qualifyAccess, setAndRaiseMethod, assignment.Left, assignment.Right, nameExpression)));
                                },
                                setAndRaiseMethod.MetadataName,
                                diagnostic);
                        }
                    }
                    else if (onPropertyChangedStatement.Parent == body &&
                             Property.TryFindSingleSetAndRaise(setter, semanticModel, context.CancellationToken, out var setAndRaise))
                    {
                        if (setAndRaise.Parent is ExpressionStatementSyntax setAndRaiseStatement &&
                            body.Statements.IndexOf(setAndRaiseStatement) == body.Statements.IndexOf(onPropertyChangedStatement) - 1)
                        {
                            context.RegisterCodeFix(
                                "Check that value is different before notifying.",
                                (editor, __) =>
                                {
                                    editor.RemoveNode(onPropertyChangedStatement);
                                    _ = editor.ReplaceNode(
                                        setAndRaiseStatement,
                                        x => InpcFactory.IfStatement(
                                            x.Expression.WithoutTrivia(),
                                            onPropertyChangedStatement));
                                },
                                nameof(CheckIfDifferentBeforeNotifyFix),
                                diagnostic);
                        }
                        else if (setAndRaise.Parent is IfStatementSyntax ifSetAndRaiseStatement &&
                                 body.Statements.IndexOf(ifSetAndRaiseStatement) == body.Statements.IndexOf(onPropertyChangedStatement) - 1)
                        {
                            context.RegisterCodeFix(
                                "Check that value is different before notifying.",
                                (editor, _) =>
                                {
                                    editor.MoveOnPropertyChangedInside(ifSetAndRaiseStatement, onPropertyChangedStatement);
                                },
                                nameof(CheckIfDifferentBeforeNotifyFix),
                                diagnostic);
                        }
                    }
                }
            }
        }
    }
}
