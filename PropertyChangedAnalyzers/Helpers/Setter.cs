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

        internal static bool TryFindSingleAssignment(AccessorDeclarationSyntax setter, [NotNullWhen(true)] out AssignmentExpressionSyntax? assignment)
        {
            assignment = null;
            if (setter is null)
            {
                return false;
            }

            using (var walker = AssignmentWalker.Borrow(setter))
            {
                if (walker.Assignments.TrySingle<AssignmentExpressionSyntax>(out assignment) &&
                    assignment.Right is IdentifierNameSyntax identifierName &&
                    identifierName.Identifier.ValueText == "value")
                {
                    return true;
                }
            }

            assignment = null;
            return false;
        }

        internal static ExpressionSyntax? FindSingleMutated(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using (var mutations = MutationWalker.Borrow(setter, SearchScope.Member, semanticModel, cancellationToken))
            {
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
            }

            return null;
        }

        internal static BackingMemberMutation? MatchMutation(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return MatchAssign(candidate, semanticModel, cancellationToken) ??
                   MatchTrySet(candidate, semanticModel, cancellationToken);
        }

        internal static BackingMemberMutation? MatchAssign(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return candidate switch
            {
                AssignmentExpressionSyntax { Left: { } left, Right: { } right }
                when FindParameter(right) is { } parameter &&
                     IsMember(left) => new BackingMemberMutation(left, parameter),
                _ => null
            };

            bool IsMember(ExpressionSyntax expression)
            {
                return semanticModel.TryGetSymbol(expression, cancellationToken, out var symbol) &&
                       symbol.IsEitherKind(SymbolKind.Field, SymbolKind.Property);
            }
        }

        internal static BackingMemberMutation? MatchTrySet(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return candidate switch
            {
                InvocationExpressionSyntax { ArgumentList: { Arguments: { } } } invocation
                when TrySet.Match(invocation, semanticModel, cancellationToken) is { Field: { } field, Value: { } value } &&
                     FindParameter(value.Expression) is { } parameter
                => new BackingMemberMutation(field.Expression, parameter),
                _ => null
            };
        }

        internal static IFieldSymbol? FindBackingField(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            switch (FindSingleMutated(setter, semanticModel, cancellationToken))
            {
                case IdentifierNameSyntax identifierName:
                    return semanticModel.TryGetSymbol(identifierName, cancellationToken, out IFieldSymbol? field) ? field : null;
                case MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _, Name: { } name }:
                    return semanticModel.TryGetSymbol(name, cancellationToken, out field) ? field : null;
                default:
                    return null;
            }
        }

        internal static AssignmentExpressionSyntax? AssignsValueToBackingField(AccessorDeclarationSyntax setter)
        {
            using (var walker = AssignmentWalker.Borrow(setter))
            {
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
                _ => null
            };
        }
    }
}
