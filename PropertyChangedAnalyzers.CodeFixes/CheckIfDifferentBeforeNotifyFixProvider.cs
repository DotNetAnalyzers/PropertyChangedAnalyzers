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
    using PropertyChangedAnalyzers.Helpers;

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

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var invocationStatement = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                    .FirstAncestorOrSelf<StatementSyntax>();
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
    }
}