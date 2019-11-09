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
            if (setter == null)
            {
                return false;
            }

            using (var walker = InvocationWalker.Borrow(setter))
            {
                return walker.Invocations.TrySingle<InvocationExpressionSyntax>(x => TrySet.IsMatch(x, semanticModel, cancellationToken) != AnalysisResult.No, out invocation);
            }
        }

        internal static bool TryFindSingleAssignment(AccessorDeclarationSyntax setter, [NotNullWhen(true)] out AssignmentExpressionSyntax? assignment)
        {
            assignment = null;
            if (setter == null)
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

        internal static bool TryFindSingleMutation(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ExpressionSyntax? backing)
        {
            backing = null;
            return property.TryGetSetter(out var setter) &&
                   TryFindSingleMutation(setter, semanticModel, cancellationToken, out backing);
        }

        internal static bool TryFindSingleMutation(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ExpressionSyntax? backing)
        {
            backing = null;
            using (var mutations = MutationWalker.Borrow(setter, SearchScope.Member, semanticModel, cancellationToken))
            {
                if (mutations.All().TrySingle(x => x.IsEither(SyntaxKind.SimpleAssignmentExpression, SyntaxKind.Argument), out var mutation))
                {
                    return mutation switch
                    {
                        AssignmentExpressionSyntax assignment => IsMutation(assignment, semanticModel, cancellationToken, out _, out backing),
                        ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } } => IsMutation(invocation, semanticModel, cancellationToken, out _, out backing),
                        _ => false,
                    };
                }
            }

            return false;
        }

        internal static bool IsMutation(ExpressionSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ExpressionSyntax? parameter, [NotNullWhen(true)] out ExpressionSyntax? backing)
        {
            switch (candidate)
            {
                case AssignmentExpressionSyntax { Left: { } left, Right: { } right }
                    when IsParameter(right, out parameter) &&
                         IsMember(left):
                    backing = left;
                    return true;
                case InvocationExpressionSyntax { ArgumentList: { Arguments: { } arguments } } invocation
                    when arguments.TrySingle(x => x.RefOrOutKeyword.IsKind(SyntaxKind.None) && IsParameter(x.Expression, out _), out var parameterArg) &&
                         arguments.TrySingle(x => x.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword), out var refArgument) &&
                         IsMember(refArgument.Expression):
                    backing = refArgument.Expression;
                    parameter = parameterArg.Expression;
                    return TrySet.IsMatch(invocation, semanticModel, cancellationToken) == AnalysisResult.Yes;
                default:
                    parameter = null;
                    backing = null;
                    return false;
            }

            static bool IsParameter(ExpressionSyntax expression, out ExpressionSyntax result)
            {
                switch (expression)
                {
                    case IdentifierNameSyntax identifierName when identifierName.Identifier.ValueText == "value":
                        result = identifierName;
                        return true;
                    case CastExpressionSyntax cast:
                        return IsParameter(cast.Expression, out result);
                    default:
                        result = null;
                        return false;
                }
            }

            bool IsMember(ExpressionSyntax expression)
            {
                return semanticModel.TryGetSymbol(expression, cancellationToken, out var symbol) &&
                       symbol.IsEitherKind(SymbolKind.Field, SymbolKind.Property);
            }
        }

        internal static bool IsMutation(ExpressionStatementSyntax candidate, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ExpressionSyntax? parameter, [NotNullWhen(true)] out ExpressionSyntax? backing)
        {
            return IsMutation(candidate.Expression, semanticModel, cancellationToken, out parameter, out backing);
        }

        internal static bool TryGetBackingField(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IFieldSymbol? field)
        {
            if (TryFindSingleMutation(setter, semanticModel, cancellationToken, out var mutated))
            {
                switch (mutated)
                {
                    case IdentifierNameSyntax _:
                        return semanticModel.TryGetSymbol(mutated, cancellationToken, out field);
                    case MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _ }:
                        return semanticModel.TryGetSymbol(mutated, cancellationToken, out field);
                }
            }

            field = null;
            return false;
        }

        internal static bool AssignsValueToBackingField(AccessorDeclarationSyntax setter, [NotNullWhen(true)] out AssignmentExpressionSyntax? assignment)
        {
            using (var walker = AssignmentWalker.Borrow(setter))
            {
                foreach (var a in walker.Assignments)
                {
                    if ((a.Right as IdentifierNameSyntax)?.Identifier.ValueText != "value")
                    {
                        continue;
                    }

                    if (a.Left is IdentifierNameSyntax)
                    {
                        assignment = a;
                        return true;
                    }

                    if (a.Left is MemberAccessExpressionSyntax { Name: IdentifierNameSyntax _ } memberAccess)
                    {
                        if (memberAccess.Expression is ThisExpressionSyntax ||
                            memberAccess.Expression is IdentifierNameSyntax)
                        {
                            assignment = a;
                            return true;
                        }

                        if (memberAccess.Expression is MemberAccessExpressionSyntax nested &&
                            nested.Expression is ThisExpressionSyntax &&
                            nested.Name is IdentifierNameSyntax)
                        {
                            assignment = a;
                            return true;
                        }
                    }
                }
            }

            assignment = null;
            return false;
        }
    }
}
