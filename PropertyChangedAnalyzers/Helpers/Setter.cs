namespace PropertyChangedAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class SetAccessor
    {
        internal static bool TryFindSingleTrySet(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax invocation)
        {
            invocation = null;
            if (setter == null)
            {
                return false;
            }

            using (var walker = InvocationWalker.Borrow(setter))
            {
                return walker.Invocations.TrySingle(x => TrySet.IsMatch(x, semanticModel, cancellationToken) != AnalysisResult.No, out invocation);
            }
        }

        internal static bool TryFindSingleAssignment(AccessorDeclarationSyntax setter, out AssignmentExpressionSyntax assignment)
        {
            assignment = null;
            if (setter == null)
            {
                return false;
            }

            using (var walker = AssignmentWalker.Borrow(setter))
            {
                if (walker.Assignments.TrySingle(out assignment) &&
                    assignment.Right is IdentifierNameSyntax identifierName &&
                    identifierName.Identifier.ValueText == "value")
                {
                    return true;
                }
            }

            assignment = null;
            return false;
        }

        internal static bool AssignsValueToBackingField(AccessorDeclarationSyntax setter, out AssignmentExpressionSyntax assignment)
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

                    if (a.Left is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name is IdentifierNameSyntax)
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

        internal static bool TryFindSingleMutationWithParameter(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken, out ExpressionSyntax fieldAccess)
        {
            using (var mutations = MutationWalker.Borrow(setter, SearchScope.Member, semanticModel, cancellationToken))
            {
                if (mutations.TrySingle(out var mutation))
                {
                    switch (mutation)
                    {
                        case AssignmentExpressionSyntax assignment:
                            fieldAccess = assignment.Left;
                            return assignment.Right is IdentifierNameSyntax identifierName &&
                                   identifierName.Identifier.ValueText == "value";
                        case ArgumentSyntax argument:
                            fieldAccess = null;
                            return argument.Parent is ArgumentListSyntax argumentList &&
                                   argumentList.Parent is InvocationExpressionSyntax invocation &&
                                   argumentList.Arguments.TrySingle(x => x.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword), out var refArgument) &&
                                   (fieldAccess = refArgument.Expression) != null &&
                                   TrySet.IsMatch(invocation, semanticModel, cancellationToken) == AnalysisResult.Yes;
                        default:
                            fieldAccess = null;
                            return false;
                    }
                }
            }

            fieldAccess = null;
            return false;
        }
    }
}
