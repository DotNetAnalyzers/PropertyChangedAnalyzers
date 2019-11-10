namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class InvocationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC005CheckIfDifferentBeforeNotifying,
            Descriptors.INPC009DoNotRaiseChangeForMissingProperty);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax invocation &&
                PropertyChanged.TryGetName(invocation, context.SemanticModel, context.CancellationToken, out _) != AnalysisResult.No)
            {
                if (invocation.TryFirstAncestor(out AccessorDeclarationSyntax? setter) &&
                    setter.IsKind(SyntaxKind.SetAccessorDeclaration))
                {
                    if (Setter.TryFindSingleAssignment(setter, out var assignment))
                    {
                        if (IsFirstCall(invocation) &&
                            IncorrectOrMissingCheckIfDifferent(context, setter, invocation, assignment))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC005CheckIfDifferentBeforeNotifying, GetLocation()));
                        }
                    }
                    else if (Setter.TryFindSingleTrySet(setter, context.SemanticModel, context.CancellationToken, out var trySet))
                    {
                        if (IsFirstCall(invocation) &&
                            IncorrectOrMissingCheckIfDifferent(trySet, invocation))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC005CheckIfDifferentBeforeNotifying, GetLocation()));
                        }
                    }
                }
                else if (invocation.ArgumentList is { Arguments: { Count: 0 } })
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC009DoNotRaiseChangeForMissingProperty, invocation.GetLocation()));
                }
            }

            Location GetLocation()
            {
                if (context.Node.FirstAncestor<StatementSyntax>() is { } statement)
                {
                    return statement.GetLocation();
                }

                return context.Node.GetLocation();
            }
        }

        private static bool IsFirstCall(InvocationExpressionSyntax invocation)
        {
            if (invocation.FirstAncestorOrSelf<BlockSyntax>() is { } block &&
                invocation.TryGetMethodName(out var name))
            {
                using (var walker = InvocationWalker.Borrow(block))
                {
                    foreach (var other in walker.Invocations)
                    {
                        if (block != other.Parent?.Parent)
                        {
                            continue;
                        }

                        if (other.SpanStart >= invocation.SpanStart)
                        {
                            return true;
                        }

                        if (other.TryGetMethodName(out var otherName) &&
                            name == otherName)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static bool IncorrectOrMissingCheckIfDifferent(SyntaxNodeAnalysisContext context, AccessorDeclarationSyntax setter, InvocationExpressionSyntax invocation, AssignmentExpressionSyntax assignment)
        {
            if (context.ContainingSymbol is IMethodSymbol { Parameters: { Length: 1 } parameters, AssociatedSymbol: IPropertySymbol property } &&
                parameters.TrySingle(out var value))
            {
                using (var walker = IfStatementWalker.Borrow(setter))
                {
                    if (walker.IfStatements.Count == 0)
                    {
                        return true;
                    }

                    var backingField = context.SemanticModel.GetSymbolSafe(assignment.Left, context.CancellationToken);
                    foreach (var ifStatement in walker.IfStatements)
                    {
                        if (ifStatement.SpanStart >= invocation.SpanStart)
                        {
                            continue;
                        }

                        if (IsEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, backingField) ||
                            IsEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, property))
                        {
                            if (ifStatement.Statement.Span.Contains(invocation.Span))
                            {
                                return true;
                            }

                            if (ifStatement.IsReturnIfTrue())
                            {
                                return false;
                            }

                            continue;
                        }

                        if (IsNegatedEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, backingField) ||
                            IsNegatedEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, property))
                        {
                            if (!ifStatement.Statement.Span.Contains(invocation.Span) ||
                                ifStatement.IsReturnIfTrue())
                            {
                                return true;
                            }

                            return false;
                        }

                        if (UsesValueAndMember(ifStatement, context.SemanticModel, value, backingField, context.CancellationToken) ||
                            UsesValueAndMember(ifStatement, context.SemanticModel, value, property, context.CancellationToken))
                        {
                            if (ifStatement.Statement.Span.Contains(invocation.Span) ||
                                ifStatement.IsReturnIfTrue())
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private static bool IncorrectOrMissingCheckIfDifferent(InvocationExpressionSyntax setAndRaise, InvocationExpressionSyntax invocation)
        {
            if (setAndRaise.Parent is IfStatementSyntax ifStatement)
            {
                return !ifStatement.Statement.Contains(invocation);
            }

            if (setAndRaise.Parent is PrefixUnaryExpressionSyntax unary &&
                     unary.IsKind(SyntaxKind.LogicalNotExpression) &&
                     unary.Parent is IfStatementSyntax ifNegated)
            {
                return ifNegated.Span.Contains(invocation.Span);
            }

            if (setAndRaise.Parent is StatementSyntax)
            {
                return true;
            }

            return false;
        }

        private static bool IsNegatedEqualsCheck(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            if (expression is PrefixUnaryExpressionSyntax unary &&
                unary.IsKind(SyntaxKind.LogicalNotExpression))
            {
                return IsEqualsCheck(unary.Operand, semanticModel, cancellationToken, value, member);
            }

            return Equality.IsOperatorNotEquals(expression, semanticModel, value, member, cancellationToken);
        }

        private static bool IsEqualsCheck(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            if (expression is InvocationExpressionSyntax equals)
            {
                if (Equality.IsObjectEquals(equals, semanticModel, value, member, cancellationToken) ||
                    Equality.IsInstanceEquals(equals, semanticModel, value, member, cancellationToken) ||
                    Equality.IsInstanceEquals(equals, semanticModel, member, value, cancellationToken) ||
                    Equality.IsReferenceEquals(equals, semanticModel, value, member, cancellationToken) ||
                    Equality.IsEqualityComparerEquals(equals, semanticModel, cancellationToken, value, member) ||
                    Equality.IsStringEquals(equals, semanticModel, value, member, cancellationToken) ||
                    Equality.IsNullableEquals(equals, semanticModel, value, member, cancellationToken))
                {
                    return true;
                }

                return false;
            }

            return Equality.IsOperatorEquals(expression, semanticModel, value, member, cancellationToken);
        }

        private static bool UsesValueAndMember(IfStatementSyntax ifStatement, SemanticModel semanticModel, IParameterSymbol value, ISymbol member, CancellationToken cancellationToken)
        {
            var usesValue = false;
            var usesMember = false;
            using (var walker = IdentifierNameWalker.Borrow(ifStatement.Condition))
            {
                foreach (var identifierName in walker.IdentifierNames)
                {
                    var symbol = semanticModel.GetSymbolSafe(identifierName, cancellationToken);
                    if (symbol == null)
                    {
                        continue;
                    }

                    if (symbol.Equals(value))
                    {
                        usesValue = true;
                    }

                    if (symbol.Equals(member))
                    {
                        usesMember = true;
                    }
                }
            }

            return usesMember && usesValue;
        }
    }
}
