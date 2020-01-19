namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
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
            Descriptors.INPC006UseObjectEqualsForReferenceTypes.Id,
            Descriptors.INPC023InstanceEquals.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out ExpressionSyntax? expression))
                {
                    if (TryGetMethodName() is { } name)
                    {
                        switch (expression)
                        {
                            case InvocationExpressionSyntax { Expression: { } e, ArgumentList: { Arguments: { Count: 2 } } }:
                                context.RegisterCodeFix(
                                    $"Use {name}",
                                    editor => editor.ReplaceNode(
                                        e,
                                        x => SyntaxFactory.IdentifierName(name).WithTriviaFrom(x)),
                                    nameof(EqualityFix),
                                    diagnostic);
                                break;
                            case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } left }, ArgumentList: { Arguments: { Count: 1 } arguments } } invocation:
                                context.RegisterCodeFix(
                                    $"Use {name}",
                                    editor => editor.ReplaceNode(
                                        invocation,
                                        x => InpcFactory.Equals(null, name, left, arguments[0].Expression).WithTriviaFrom(x)),
                                    nameof(EqualityFix),
                                    diagnostic);
                                break;
                            case BinaryExpressionSyntax { Left: { }, OperatorToken: { ValueText: "==" }, Right: { } } binary:
                                context.RegisterCodeFix(
                                    $"Use {name}",
                                    editor => editor.ReplaceNode(
                                        binary,
                                        x => InpcFactory.Equals(null, name, x.Left.WithoutTrivia(), x.Right.WithoutTrivia()).WithTriviaFrom(x)),
                                    nameof(EqualityFix),
                                    diagnostic);
                                break;
                            case BinaryExpressionSyntax { Left: { }, OperatorToken: { ValueText: "!=" }, Right: { } } binary:
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
                    else if (expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } left }, ArgumentList: { Arguments: { Count: 1 } arguments } } invocation &&
                             diagnostic.Id == Descriptors.INPC023InstanceEquals.Id)
                    {
                        context.RegisterCodeFix(
                            "Use null safe equals.",
                            (editor, cancellationToken) => editor.ReplaceNode(
                                invocation,
                                x => InpcFactory.Equals(left, arguments[0].Expression, editor.SemanticModel, cancellationToken).WithTriviaFrom(x)),
                            Descriptors.INPC023InstanceEquals.Id,
                            diagnostic);
                    }
                }

                string? TryGetMethodName()
                {
                    if (diagnostic.Id == Descriptors.INPC006UseReferenceEqualsForReferenceTypes.Id)
                    {
                        return nameof(ReferenceEquals);
                    }

                    if (diagnostic.Id == Descriptors.INPC006UseObjectEqualsForReferenceTypes.Id)
                    {
                        return nameof(Equals);
                    }

                    return null!;
                }
            }
        }
    }
}
