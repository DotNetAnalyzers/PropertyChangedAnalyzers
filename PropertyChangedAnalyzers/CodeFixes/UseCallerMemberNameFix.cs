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
    using Microsoft.CodeAnalysis.Simplification;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCallerMemberNameFix))]
    [Shared]
    internal class UseCallerMemberNameFix : DocumentEditorCodeFixProvider
    {
        private static readonly AttributeListSyntax CallerMemberName =
            SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.ParseName("System.Runtime.CompilerServices.CallerMemberName")
                                     .WithAdditionalAnnotations(Simplifier.Annotation))));

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC004UseCallerMemberName.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
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

                var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
                if (node.FirstAncestorOrSelf<ParameterSyntax>() is ParameterSyntax parameter)
                {
                    context.RegisterCodeFix(
                        "Use [CallerMemberName]",
                        (editor, x) => editor.ReplaceNode(
                            parameter,
                            AsCallerMemberName(parameter)),
                        this.GetType(),
                        diagnostic);
                    continue;
                }

                if (node.FirstAncestorOrSelf<ArgumentSyntax>() is ArgumentSyntax argument)
                {
                    if (argument.Parent.Parent is InvocationExpressionSyntax invocation &&
                        semanticModel.GetSymbolSafe(invocation, context.CancellationToken) is IMethodSymbol method &&
                        method.TryFindParameter(argument, out var parameterSymbol))
                    {
                        if (parameterSymbol.IsCallerMemberName())
                        {
                            context.RegisterCodeFix(
                            "Use [CallerMemberName]",
                            (editor, _) => editor.RemoveNode(argument),
                            this.GetType(),
                            diagnostic);
                        }
                        else if (parameterSymbol.TrySingleDeclaration(context.CancellationToken, out ParameterSyntax parameterSyntax))
                        {
                            context.RegisterCodeFix(
                                "Use [CallerMemberName]",
                                (editor, x) =>
                                {
                                    editor.ReplaceNode(
                                        parameterSyntax,
                                        AsCallerMemberName(parameterSyntax));
                                    editor.RemoveNode(argument);
                                },
                                this.GetType(),
                                diagnostic);
                        }
                    }
                }
            }
        }

        private static ParameterSyntax AsCallerMemberName(ParameterSyntax parameter)
        {
            return parameter.AddAttributeLists(CallerMemberName)
                            .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("null")));
        }
    }
}
