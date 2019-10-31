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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCallerMemberNameFix))]
    [Shared]
    internal class UseCallerMemberNameFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC004UseCallerMemberName.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ParameterSyntax? parameter))
                {
                    context.RegisterCodeFix(
                        "Use [CallerMemberName]",
                        (editor, x) => editor.ReplaceNode(
                            parameter,
                            AsCallerMemberName(parameter)),
                        nameof(UseCallerMemberNameFix),
                        diagnostic);
                }
                else if (syntaxRoot.TryFindNode(diagnostic, out ArgumentSyntax? argument) &&
                         argument.Parent is ArgumentListSyntax argumentList &&
                         argumentList.Parent is InvocationExpressionSyntax invocation &&
                         semanticModel.TryGetSymbol(invocation, context.CancellationToken, out var method) &&
                         method.TryFindParameter(argument, out var parameterSymbol))
                {
                    if (parameterSymbol.IsCallerMemberName())
                    {
                        context.RegisterCodeFix(
                            "Use [CallerMemberName]",
                            (editor, _) => editor.RemoveNode(argument),
                            nameof(UseCallerMemberNameFix),
                            diagnostic);
                    }
                    else if (parameterSymbol.TrySingleDeclaration(context.CancellationToken, out parameter))
                    {
                        context.RegisterCodeFix(
                            "Use [CallerMemberName]",
                            (editor, x) =>
                            {
                                editor.ReplaceNode(
                                    parameter,
                                    AsCallerMemberName(parameter));
                                editor.RemoveNode(argument);
                            },
                            nameof(UseCallerMemberNameFix),
                            diagnostic);
                    }
                }
            }
        }

        private static ParameterSyntax AsCallerMemberName(ParameterSyntax parameter)
        {
            return parameter.AddAttributeLists(InpcFactory.CallerMemberNameAttributeList)
                            .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));
        }
    }
}
