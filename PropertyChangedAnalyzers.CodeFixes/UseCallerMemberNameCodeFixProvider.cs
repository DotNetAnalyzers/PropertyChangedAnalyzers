namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Simplification;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCallerMemberNameCodeFixProvider))]
    [Shared]
    internal class UseCallerMemberNameCodeFixProvider : CodeFixProvider
    {
        private static readonly AttributeListSyntax CallerMemberName =
            SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.ParseName("System.Runtime.CompilerServices.CallerMemberName")
                                     .WithAdditionalAnnotations(Simplifier.Annotation))));

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC004UseCallerMemberName.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

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

                var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
                if (node.FirstAncestorOrSelf<ParameterSyntax>() is ParameterSyntax parameter)
                {
                    context.RegisterDocumentEditorFix(
                        "Use [CallerMemberName]",
                        (editor, _) => MakeUseCallerMemberName(editor, parameter, AsCallerMemberName(parameter)),
                        this.GetType(),
                        diagnostic);
                    continue;
                }

                if (node.FirstAncestorOrSelf<ArgumentSyntax>() is ArgumentSyntax argument)
                {
                    context.RegisterDocumentEditorFix(
                            "Use [CallerMemberName]",
                            (editor, _) => editor.RemoveNode(argument),
                            this.GetType(),
                            diagnostic);
                }
            }
        }

        private static void MakeUseCallerMemberName(DocumentEditor editor, SyntaxNode oldNode, SyntaxNode newNode)
        {
            editor.ReplaceNode(oldNode, (_, __) => newNode);
        }

        private static ParameterSyntax AsCallerMemberName(ParameterSyntax parameter)
        {
            return parameter.AddAttributeLists(CallerMemberName)
                            .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("null")));
        }
    }
}
