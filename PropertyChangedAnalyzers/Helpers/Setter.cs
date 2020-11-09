namespace PropertyChangedAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Setter
    {
        internal static bool TryFindSingleTrySet(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out InvocationExpressionSyntax? invocation)
        {
            invocation = null;
            if (setter is null)
            {
                return false;
            }

            using var walker = InvocationWalker.Borrow(setter);
            return walker.Invocations.TrySingle<InvocationExpressionSyntax>(x => TrySet.IsMatch(x, semanticModel, cancellationToken) != AnalysisResult.No, out invocation);
        }

        internal static AssignmentExpressionSyntax? FindSingleAssignment(AccessorDeclarationSyntax setter)
        {
            using var walker = AssignmentWalker.Borrow(setter);
            if (walker.Assignments.TrySingle<AssignmentExpressionSyntax>(out var assignment) &&
                assignment.Right is IdentifierNameSyntax { Identifier: { ValueText: "value" } })
            {
                return assignment;
            }

            return null;
        }

        internal static ExpressionSyntax? FindSingleMutated(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using var mutations = MutationWalker.Borrow(setter, SearchScope.Member, semanticModel, cancellationToken);
            if (mutations.All().TrySingle(x => x.IsEither(SyntaxKind.SimpleAssignmentExpression, SyntaxKind.Argument), out var mutation))
            {
                return mutation switch
                {
                    AssignmentExpressionSyntax assignment
                        when MatchMutation(assignment, semanticModel, cancellationToken) is { Member: { } backingMember }
                        => backingMember,
                    ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } }
                        when MatchMutation(invocation, semanticModel, cancellationToken) is { Member: { } backingMember }
                        => backingMember,
                    _ => null,
                };
            }

            return null;
        }

        internal static BackingMemberAndValue? MatchMutation(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return MatchAssign(candidate, semanticModel, cancellationToken) ??
                   MatchTrySet(candidate, semanticModel, cancellationToken);
        }

        internal static BackingMemberAndValue? MatchAssign(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return candidate switch
            {
                AssignmentExpressionSyntax { Left: { } left, Right: { } right }
                => MatchMemberAndParameter(left, right, semanticModel, cancellationToken),
                _ => null,
            };
        }

        internal static BackingMemberAndValue? MatchTrySet(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return candidate switch
            {
                InvocationExpressionSyntax { ArgumentList: { Arguments: { } } } invocation
                when TrySet.Match(invocation, semanticModel, cancellationToken) is { Field: { } field, Value: { } value }
                => MatchMemberAndParameter(field.Expression, value.Expression, semanticModel, cancellationToken),
                _ => null,
            };
        }

        internal static BackingMemberAndValue? MatchEquals(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (candidate is PrefixUnaryExpressionSyntax { OperatorToken: { ValueText: "!" }, Operand: { } condition } &&
                Equality.IsEqualsCheck(condition, semanticModel, cancellationToken, out _, out _))
            {
                return MatchEquals(condition, semanticModel, cancellationToken);
            }

            if (Equality.IsEqualsCheck(candidate, semanticModel, cancellationToken, out var left, out var right))
            {
                return MatchMemberAndParameter(left, right, semanticModel, cancellationToken) ??
                       MatchMemberAndParameter(right, left, semanticModel, cancellationToken);
            }

            return null;
        }

        internal static IFieldSymbol? FindBackingField(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return FindSingleMutated(setter, semanticModel, cancellationToken) switch
            {
                IdentifierNameSyntax identifierName
                    => semanticModel.TryGetSymbol(identifierName, cancellationToken, out IFieldSymbol? field)
                    ? field
                    : null,
                MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _, Name: { } name }
                    => semanticModel.TryGetSymbol(name, cancellationToken, out IFieldSymbol? field) ? field : null,
                _ => null,
            };
        }

        internal static AssignmentExpressionSyntax? AssignsValueToBackingField(AccessorDeclarationSyntax setter)
        {
            using var walker = AssignmentWalker.Borrow(setter);
            foreach (var assignment in walker.Assignments)
            {
                if (assignment is { Right: IdentifierNameSyntax { Identifier: { ValueText: "value" } } })
                {
                    switch (assignment.Left)
                    {
                        case IdentifierNameSyntax _:
                        case MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _ }:
                        case MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax _ }:
                        case MemberAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax _ } }:
                        case MemberAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _ } }:
                            return assignment;
                    }
                }
            }

            return null;
        }

        internal static BackingMemberAndValue? MatchMemberAndParameter(ExpressionSyntax memberCandidate, ExpressionSyntax parameterCandidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (FindParameter(parameterCandidate) is { } parameter &&
                IsMember(memberCandidate, semanticModel, cancellationToken))
            {
                return new BackingMemberAndValue(memberCandidate, parameter);
            }

            return null;
        }

        private static IdentifierNameSyntax? FindParameter(ExpressionSyntax expression)
        {
            return expression switch
            {
                IdentifierNameSyntax { Identifier: { ValueText: "value" } } identifierName
                => identifierName,
                CastExpressionSyntax cast => FindParameter(cast.Expression),
                _ => null,
            };
        }

        private static bool IsMember(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return semanticModel.TryGetSymbol(expression, cancellationToken, out var symbol) &&
                   symbol.IsEitherKind(SymbolKind.Field, SymbolKind.Property);
        }
    }
}
