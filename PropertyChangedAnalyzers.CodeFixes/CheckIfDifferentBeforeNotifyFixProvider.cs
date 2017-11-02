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
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CheckIfDifferentBeforeNotifyFixProvider))]
    [Shared]
    internal class CheckIfDifferentBeforeNotifyFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC005CheckIfDifferentBeforeNotifying.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                      .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var invocationStatement = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                    .FirstAncestorOrSelf<ExpressionStatementSyntax>();
                var setter = invocationStatement?.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
                if (setter?.IsKind(SyntaxKind.SetAccessorDeclaration) != true ||
                    setter.Body == null)
                {
                    continue;
                }

                if (Property.TryGetSingleAssignmentInSetter(setter, out var assignment))
                {
                    var statementSyntax = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
                    if (setter.Body.Statements.First() != statementSyntax)
                    {
                        continue;
                    }

                    context.RegisterDocumentEditorFix(
                        "Check that value is different before notifying.",
                        (editor, cancellationToken) => AddCheckIfDifferent(editor, assignment, cancellationToken),
                        this.GetType(),
                        diagnostic);

                    var type = semanticModel.GetDeclaredSymbolSafe(
                        setter.FirstAncestor<ClassDeclarationSyntax>(),
                        context.CancellationToken);
                    if (setter.Body.Statements.Count == 2 &&
                        ReferenceEquals(setter.Body.Statements[0], statementSyntax) &&
                        PropertyChanged.TryGetSetAndRaiseMethod(type, semanticModel, context.CancellationToken, out var setAndRaiseMethod))
                    {
                        context.RegisterDocumentEditorFix(
                            $"Use {setAndRaiseMethod.ContainingType.MetadataName}.{setAndRaiseMethod.MetadataName}",
                            (editor, cancellationToken) => UseSetAndRaise(editor, setter, assignment, setAndRaiseMethod, cancellationToken),
                            $"Use {setAndRaiseMethod.ContainingType.MetadataName}.{setAndRaiseMethod.MetadataName}",
                            diagnostic);
                    }
                }

                if (Property.TryGetSingleSetAndRaiseInSetter(setter, semanticModel, context.CancellationToken, out var setAndRaise))
                {
                    if (setAndRaise.Parent is ExpressionStatementSyntax setAndRaiseStatement)
                    {
                        if (invocationStatement.Parent is BlockSyntax block &&
                            block.Statements.IndexOf(setAndRaiseStatement) == block.Statements.IndexOf(invocationStatement) - 1)
                        {
                            context.RegisterDocumentEditorFix(
                                    "Check that value is different before notifying.",
                                    (editor, cancellationToken) => CreateIf(editor, setAndRaiseStatement, invocationStatement),
                                    this.GetType(),
                                diagnostic);
                        }
                    }
                    else if (setAndRaise.Parent is IfStatementSyntax ifSetAndRaiseStatement)
                    {
                        if (invocationStatement.Parent is BlockSyntax block &&
                            block.Statements.IndexOf(ifSetAndRaiseStatement) == block.Statements.IndexOf(invocationStatement) - 1)
                        {
                            context.RegisterDocumentEditorFix(
                                "Check that value is different before notifying.",
                                (editor, cancellationToken) => AddToIf(editor, ifSetAndRaiseStatement, invocationStatement),
                                this.GetType(),
                                diagnostic);
                        }
                    }
                }
            }
        }

        private static void AddCheckIfDifferent(DocumentEditor editor, AssignmentExpressionSyntax assignment, CancellationToken cancellationToken)
        {
            var statementSyntax = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
            if (statementSyntax == null)
            {
                return;
            }

            var type = editor.SemanticModel.GetTypeInfoSafe(assignment.Left, cancellationToken).Type;
            var code = StringBuilderPool.Borrow()
                                        .AppendLine($"if ({Snippet.EqualityCheck(type, "value", assignment.Left.ToString(), editor.SemanticModel)})")
                                        .AppendLine("{")
                                        .AppendLine("   return;")
                                        .AppendLine("}")
                                        .AppendLine()
                                        .Return();
            var ifReturn = SyntaxFactory.ParseStatement(code)
                                        .WithSimplifiedNames()
                                        .WithLeadingElasticLineFeed()
                                        .WithTrailingElasticLineFeed()
                                        .WithAdditionalAnnotations(Formatter.Annotation);
            editor.InsertBefore(statementSyntax, ifReturn);
        }

        private static void CreateIf(DocumentEditor editor, ExpressionStatementSyntax setAndRaise, ExpressionStatementSyntax invocation)
        {
            editor.RemoveNode(setAndRaise);
            editor.ReplaceNode(
                invocation,
                (node, _) =>
                {
                    var code = StringBuilderPool.Borrow()
                                                .AppendLine($"if ({setAndRaise.ToFullString().TrimEnd('\r', '\n', ';')})")
                                                .AppendLine("{")
                                                .AppendLine($"    {invocation.ToFullString().TrimEnd('\r', '\n')}")
                                                .AppendLine("}")
                                                .Return();

                    return SyntaxFactory.ParseStatement(code)
                                        .WithSimplifiedNames()
                                        .WithLeadingElasticLineFeed()
                                        .WithTrailingElasticLineFeed()
                                        .WithAdditionalAnnotations(Formatter.Annotation);
                });
        }

        private static void AddToIf(DocumentEditor editor, IfStatementSyntax ifSetAndRaise, ExpressionStatementSyntax invocation)
        {
            if (ifSetAndRaise.Statement is BlockSyntax body)
            {
                editor.RemoveNode(invocation);
                if (body.Statements.Count == 0)
                {
                    editor.ReplaceNode(
                        body,
                        body.AddStatements(invocation.WithLeadingElasticLineFeed()));
                }
                else
                {
                    editor.InsertAfter(body.Statements.Last(), invocation.WithLeadingElasticLineFeed());
                }
            }
            else
            {
                if (ifSetAndRaise.Statement == null)
                {
                    editor.RemoveNode(invocation);
                    editor.ReplaceNode(
                        ifSetAndRaise,
                        (x, _) => ((IfStatementSyntax)x)
                            .WithStatement(SyntaxFactory.Block(ifSetAndRaise.Statement, invocation))
                            .WithSimplifiedNames()
                            .WithTrailingElasticLineFeed()
                            .WithAdditionalAnnotations(Formatter.Annotation));
                }
                else
                {
                    editor.RemoveNode(invocation);
                    editor.ReplaceNode(
                        ifSetAndRaise.Statement,
                        (_, __) => SyntaxFactory.Block(ifSetAndRaise.Statement, invocation)
                                                .WithSimplifiedNames()
                                                .WithTrailingElasticLineFeed()
                                                .WithAdditionalAnnotations(Formatter.Annotation));
                }
            }

            editor.FormatNode(ifSetAndRaise);
        }

        private static void UseSetAndRaise(DocumentEditor editor, AccessorDeclarationSyntax setter, AssignmentExpressionSyntax assignment, IMethodSymbol setAndRaise, CancellationToken cancellationToken)
        {
            var usesUnderscoreNames = assignment.UsesUnderscore(editor.SemanticModel, cancellationToken);
            editor.ReplaceNode(
                setter,
                x => x.WithBody(null)
                      .WithExpressionBody(
                          SyntaxFactory.ArrowExpressionClause(
                              SyntaxFactory.ParseExpression(
                                  $"{(usesUnderscoreNames ? string.Empty : "this.")}{setAndRaise.Name}(ref {assignment.Left}, value);")))
                      .WithTrailingElasticLineFeed()
                      .WithAdditionalAnnotations(Formatter.Annotation));
        }
    }
}