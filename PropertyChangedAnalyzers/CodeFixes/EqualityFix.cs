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
            Descriptors.INPC023InstanceEquals.Id,
            Descriptors.INPC024ReferenceEqualsValueType.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is ExpressionSyntax node)
                {
                    switch (node)
                    {
                        case InvocationExpressionSyntax { Expression: { } oldName, ArgumentList: { Arguments: { Count: 2 } } }
                            when EqualsMethodName() is { } name:
                            context.RegisterCodeFix(
                                $"Use {name}",
                                editor => editor.ReplaceNode(
                                    oldName,
                                    x => SyntaxFactory.IdentifierName(name).WithTriviaFrom(x)),
                                name,
                                diagnostic);
                            break;
                        case InvocationExpressionSyntax { ArgumentList: { Arguments: { Count: 2 } arguments } } referenceEquals
                            when diagnostic.Id == Descriptors.INPC024ReferenceEqualsValueType.Id:
                            context.RegisterCodeFix(
                                "Use correct equality.",
                                (editor, cancellationToken) => editor.ReplaceNode(
                                    referenceEquals switch
                                    {
                                        { Parent: PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" } } negated } => negated,
                                        _ => (ExpressionSyntax)referenceEquals,
                                    },
                                    x => Negate(x, InpcFactory.Equals(arguments[0].Expression, arguments[1].Expression, editor.SemanticModel, cancellationToken))),
                                Descriptors.INPC024ReferenceEqualsValueType.Id,
                                diagnostic);

                            break;
                        case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } left }, ArgumentList: { Arguments: { Count: 1 } arguments } } invocation
                            when arguments[0] is { Expression: { } right } &&
                                 EqualsMethodName() is { } name:
                            context.RegisterCodeFix(
                                $"Use {name}",
                                editor => editor.ReplaceNode(
                                    invocation,
                                    x => InpcFactory.Equals(null, name, left, right).WithTriviaFrom(x)),
                                name,
                                diagnostic);
                            break;
                        case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } left }, ArgumentList: { Arguments: { Count: 1 } arguments } } instanceEquals
                            when arguments[0] is { Expression: { } right } &&
                                 diagnostic.Id == Descriptors.INPC023InstanceEquals.Id:

                            context.RegisterCodeFix(
                                "Use null safe equals.",
                                (editor, cancellationToken) => editor.ReplaceNode(
                                    instanceEquals switch
                                    {
                                        { Parent: PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" } } negated } => negated,
                                        _ => (ExpressionSyntax)instanceEquals,
                                    },
                                    x => Negate(x, InpcFactory.Equals(left, right, editor.SemanticModel, cancellationToken))),
                                Descriptors.INPC023InstanceEquals.Id,
                                diagnostic);
                            break;
                        case BinaryExpressionSyntax { Left: { }, OperatorToken: { ValueText: "==" }, Right: { } } binary
                            when EqualsMethodName() is { } name:
                            context.RegisterCodeFix(
                                $"Use {name}",
                                editor => editor.ReplaceNode(
                                    binary,
                                    x => InpcFactory.Equals(null, name, x.Left.WithoutTrivia(), x.Right.WithoutTrivia()).WithTriviaFrom(x)),
                                name,
                                diagnostic);
                            break;
                        case BinaryExpressionSyntax { Left: { }, OperatorToken: { ValueText: "!=" }, Right: { } } binary
                            when EqualsMethodName() is { } name:
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
                                name,
                                diagnostic);
                            break;
                    }
                }

                static ExpressionSyntax Negate(ExpressionSyntax original, ExpressionSyntax check)
                {
                    return original switch
                    {
                        PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" } }
                        when check is BinaryExpressionSyntax { OperatorToken: { ValueText: "==" } } binary
                        => binary.WithOperatorToken(SyntaxFactory.Token(SyntaxKind.ExclamationEqualsToken))
                                 .WithTriviaFrom(original),
                        PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" } } negated
                        => negated.WithOperand(check.WithTriviaFrom(negated.Operand)),
                        _ => check.WithTriviaFrom(original)!,
                    };
                }

                string? EqualsMethodName()
                {
                    if (diagnostic.Id == Descriptors.INPC006UseReferenceEqualsForReferenceTypes.Id)
                    {
                        return nameof(ReferenceEquals);
                    }

                    if (diagnostic.Id == Descriptors.INPC006UseObjectEqualsForReferenceTypes.Id)
                    {
                        return nameof(Equals);
                    }

                    return null;
                }
            }
        }
    }
}
