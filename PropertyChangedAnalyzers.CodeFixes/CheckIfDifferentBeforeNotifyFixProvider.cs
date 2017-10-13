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
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CheckIfDifferentBeforeNotifyFixProvider))]
    [Shared]
    internal class CheckIfDifferentBeforeNotifyFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC005CheckIfDifferentBeforeNotifying.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => DocumentOnlyFixAllProvider.Default;

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

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Check that value is different before notifying.",
                            cancellationToken => AddCheckIfDifferentAsync(context.Document, assignment, cancellationToken),
                            this.GetType().Name),
                        diagnostic);
                }

                if (Property.TryGetSingleSetAndRaiseInSetter(setter, semanticModel, context.CancellationToken, out var setAndRaise))
                {
                    if (setAndRaise.Parent is ExpressionStatementSyntax setAndRaiseStatement)
                    {
                        if (invocationStatement.Parent is BlockSyntax block &&
                            block.Statements.IndexOf(setAndRaiseStatement) == block.Statements.IndexOf(invocationStatement) - 1)
                        {
                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    "Check that value is different before notifying.",
                                    cancellationToken => CreateIfAsync(context.Document, setAndRaiseStatement, invocationStatement, cancellationToken),
                                    this.GetType().Name),
                                diagnostic);
                        }
                    }
                    else if (setAndRaise.Parent is IfStatementSyntax ifSetAndRaiseStatement)
                    {
                        if (invocationStatement.Parent is BlockSyntax block &&
                            block.Statements.IndexOf(ifSetAndRaiseStatement) == block.Statements.IndexOf(invocationStatement) - 1)
                        {
                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    "Check that value is different before notifying.",
                                    cancellationToken => AddToIfAsync(context.Document, ifSetAndRaiseStatement, invocationStatement, cancellationToken),
                                    this.GetType().Name),
                                diagnostic);
                        }
                    }
                }
            }
        }

        private static async Task<Document> AddCheckIfDifferentAsync(
            Document document,
            AssignmentExpressionSyntax assignment,
            CancellationToken cancellationToken)
        {
            var statementSyntax = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
            if (statementSyntax == null)
            {
                return document;
            }

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            var type = editor.SemanticModel.GetTypeInfoSafe(assignment.Left, cancellationToken).Type;
            using (var pooled = StringBuilderPool.Borrow())
            {
                var code = pooled.Item
                                 .AppendLine($"if ({Snippet.EqualityCheck(type, "value", assignment.Left.ToString(), editor.SemanticModel)})")
                                 .AppendLine("{")
                                 .AppendLine("   return;")
                                 .AppendLine("}")
                                 .AppendLine()
                                 .ToString();
                var ifReturn = SyntaxFactory.ParseStatement(code)
                                            .WithSimplifiedNames()
                                            .WithLeadingElasticLineFeed()
                                            .WithTrailingElasticLineFeed()
                                            .WithAdditionalAnnotations(Formatter.Annotation);
                editor.InsertBefore(statementSyntax, ifReturn);
                return editor.GetChangedDocument();
            }
        }

        private static async Task<Document> CreateIfAsync(Document document, ExpressionStatementSyntax setAndRaise, ExpressionStatementSyntax invocation, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            editor.RemoveNode(setAndRaise);
            editor.ReplaceNode(
                invocation,
                (node, _) =>
                {
                    using (var pooled = StringBuilderPool.Borrow())
                    {
                        var code = pooled.Item.AppendLine($"if ({setAndRaise.ToFullString().TrimEnd('\r', '\n', ';')})")
                                         .AppendLine("{")
                                         .AppendLine($"    {invocation.ToFullString().TrimEnd('\r', '\n')}")
                                         .AppendLine("}")
                                         .ToString();

                        return SyntaxFactory.ParseStatement(code)
                                            .WithSimplifiedNames()
                                            .WithLeadingElasticLineFeed()
                                            .WithTrailingElasticLineFeed()
                                            .WithAdditionalAnnotations(Formatter.Annotation);
                    }
                });
            return editor.GetChangedDocument();
        }

        private static async Task<Document> AddToIfAsync(Document document, IfStatementSyntax ifSetAndRaise, ExpressionStatementSyntax invocation, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            if (ifSetAndRaise.Statement is BlockSyntax body)
            {
                editor.RemoveNode(invocation);
                if (body.Statements.Count == 0)
                {
                    editor.ReplaceNode(body, body.AddStatements(invocation.WithLeadingElasticLineFeed()));
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
                        (x, __) => ((IfStatementSyntax)x).WithStatement(SyntaxFactory.Block(ifSetAndRaise.Statement, invocation)));
                }
                else
                {
                    editor.RemoveNode(invocation);
                    editor.ReplaceNode(
                        ifSetAndRaise.Statement,
                        (_, __) => SyntaxFactory.Block(ifSetAndRaise.Statement, invocation));
                }
            }

            return editor.GetChangedDocument();
        }
    }
}