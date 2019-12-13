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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EqualityFix))]
    [Shared]
    internal class EqualityFix : DocumentEditorCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC006UseReferenceEqualsForReferenceTypes.Id,
            Descriptors.INPC006UseObjectEqualsForReferenceTypes.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out InvocationExpressionSyntax? invocation) &&
                    invocation.ArgumentList is { } argumentList &&
                    TryGetMethodName(out var name))
                {
                    if (argumentList.Arguments.Count == 2)
                    {
                        context.RegisterCodeFix(
                            $"Use {name}",
                            editor => editor.ReplaceNode(
                                invocation.Expression,
                                x => SyntaxFactory.IdentifierName(name).WithTriviaFrom(x)),
                            nameof(EqualityFix),
                            diagnostic);
                    }
                    else if (argumentList.Arguments.TrySingle(out var argument) &&
                             invocation.Expression is MemberAccessExpressionSyntax { Expression: { } expression })
                    {
                        context.RegisterCodeFix(
                            $"Use {name}",
                            editor => editor.ReplaceNode(
                                invocation,
                                x => InpcFactory.Equals(null, name, expression, argument.Expression).WithTriviaFrom(x)),
                            nameof(EqualityFix),
                            diagnostic);
                    }
                }

                if (syntaxRoot.TryFindNode(diagnostic, out BinaryExpressionSyntax? binary) &&
                    TryGetMethodName(out name))
                {
                    switch (binary.Kind())
                    {
                        case SyntaxKind.EqualsExpression:
                            context.RegisterCodeFix(
                                $"Use {name}",
                                editor => editor.ReplaceNode(
                                    binary,
                                    x => InpcFactory.Equals(null, name, x.Left.WithoutTrivia(), x.Right.WithoutTrivia()).WithTriviaFrom(x)),
                                nameof(EqualityFix),
                                diagnostic);
                            break;
                        case SyntaxKind.NotEqualsExpression:
                            context.RegisterCodeFix(
                                $"Use !{name}",
                                editor => editor.ReplaceNode(
                                    binary,
                                    x => SyntaxFactory.PrefixUnaryExpression(
                                                          SyntaxKind.LogicalNotExpression,
                                                          InpcFactory.Equals(
                                                              null,
                                                              name,
                                                              x.Left.WithoutTrivia(),
                                                              x.Right.WithoutTrivia()))
                                                      .WithTriviaFrom(x)),
                                nameof(EqualityFix),
                                diagnostic);
                            break;
                    }
                }

                bool TryGetMethodName(out string result)
                {
                    if (diagnostic.Id == Descriptors.INPC006UseReferenceEqualsForReferenceTypes.Id)
                    {
                        result = nameof(ReferenceEquals);
                        return true;
                    }

                    if (diagnostic.Id == Descriptors.INPC006UseObjectEqualsForReferenceTypes.Id)
                    {
                        result = nameof(Equals);
                        return true;
                    }

                    result = null!;
                    return false;
                }
            }
        }
    }
}
