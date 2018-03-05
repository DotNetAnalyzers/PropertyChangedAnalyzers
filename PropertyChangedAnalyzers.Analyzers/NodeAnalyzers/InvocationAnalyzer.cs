namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class InvocationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            INPC005CheckIfDifferentBeforeNotifying.Descriptor,
            INPC009DontRaiseChangeForMissingProperty.Descriptor,
            INPC016NotifyAfterUpdate.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is InvocationExpressionSyntax invocation)
            {
                if (PropertyChanged.IsOnPropertyChanged(invocation, context.SemanticModel, context.CancellationToken))
                {
                    if ((invocation.ArgumentList == null ||
                         invocation.ArgumentList.Arguments.Count == 0) &&
                        invocation.FirstAncestor<AccessorDeclarationSyntax>() == null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(INPC009DontRaiseChangeForMissingProperty.Descriptor, GetLocation()));
                    }

                    if (invocation.FirstAncestor<AccessorDeclarationSyntax>() is AccessorDeclarationSyntax setter &&
                        setter.IsKind(SyntaxKind.SetAccessorDeclaration))
                    {
                        if (Property.TrySingleAssignmentInSetter(setter, out var assignment))
                        {
                            if (!AreInSameBlock(assignment, invocation) ||
                                assignment.SpanStart > invocation.SpanStart)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC016NotifyAfterUpdate.Descriptor, GetLocation()));
                            }

                            if (IsFirstCall(invocation) &&
                                IncorrectOrMissingCheckIfDifferent(context, setter, invocation, assignment))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC005CheckIfDifferentBeforeNotifying.Descriptor, GetLocation()));
                            }
                        }
                        else if (Property.TryFindSingleSetAndRaise(setter, context.SemanticModel, context.CancellationToken, out var setAndRaise))
                        {
                            if (!AreInSameBlock(setAndRaise, invocation) ||
                                setAndRaise.SpanStart > invocation.SpanStart)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC016NotifyAfterUpdate.Descriptor, GetLocation()));
                            }

                            if (IsFirstCall(invocation) &&
                                IncorrectOrMissingCheckIfDifferent(setAndRaise, invocation))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC005CheckIfDifferentBeforeNotifying.Descriptor, GetLocation()));
                            }
                        }
                    }
                }
                else if (PropertyChanged.IsPropertyChangedInvoke(invocation, context.SemanticModel, context.CancellationToken))
                {
                    if (invocation.FirstAncestor<AccessorDeclarationSyntax>() is AccessorDeclarationSyntax setter &&
                        setter.IsKind(SyntaxKind.SetAccessorDeclaration))
                    {
                        if (Property.TrySingleAssignmentInSetter(setter, out var assignment))
                        {
                            if (!AreInSameBlock(assignment, invocation) ||
                                assignment.SpanStart > invocation.SpanStart)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC016NotifyAfterUpdate.Descriptor, GetLocation()));
                            }

                            if (IsFirstCall(invocation) &&
                                IncorrectOrMissingCheckIfDifferent(context, setter, invocation, assignment))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC005CheckIfDifferentBeforeNotifying.Descriptor, GetLocation()));
                            }
                        }
                        else if (Property.TryFindSingleSetAndRaise(setter, context.SemanticModel, context.CancellationToken, out var setAndRaise))
                        {
                            if (!AreInSameBlock(setAndRaise, invocation) ||
                                setAndRaise.SpanStart > invocation.SpanStart)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC016NotifyAfterUpdate.Descriptor, GetLocation()));
                            }

                            if (IsFirstCall(invocation) &&
                                IncorrectOrMissingCheckIfDifferent(setAndRaise, invocation))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(INPC005CheckIfDifferentBeforeNotifying.Descriptor, GetLocation()));
                            }
                        }
                    }
                }
            }

            Location GetLocation()
            {
                if (context.Node.FirstAncestor<StatementSyntax>() is StatementSyntax statement)
                {
                    return statement.GetLocation();
                }

                return context.Node.GetLocation();
            }
        }

        private static bool AreInSameBlock(SyntaxNode node1, SyntaxNode node2)
        {
            if (node1?.FirstAncestor<BlockSyntax>() is BlockSyntax block1 &&
                node2?.FirstAncestor<BlockSyntax>() is BlockSyntax block2)
            {
                return block1 == block2 ||
                       block1.Contains(block2) ||
                       block2.Contains(block1);
            }

            return false;
        }

        private static bool IsFirstCall(InvocationExpressionSyntax invocation)
        {
            if (invocation.FirstAncestorOrSelf<BlockSyntax>() is BlockSyntax block &&
                invocation.TryGetInvokedMethodName(out var name))
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

                        if (other.TryGetInvokedMethodName(out var otherName) &&
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
            if (context.ContainingSymbol is IMethodSymbol setMethod &&
                setMethod.Parameters.TrySingle(out var value) &&
                setMethod.AssociatedSymbol is IPropertySymbol property)
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

                        if (UsesValueAndMember(ifStatement, context.SemanticModel, context.CancellationToken, value, backingField) ||
                            UsesValueAndMember(ifStatement, context.SemanticModel, context.CancellationToken, value, property))
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

            return Equality.IsOperatorNotEquals(expression, semanticModel, cancellationToken, value, member);
        }

        private static bool IsEqualsCheck(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            if (expression is InvocationExpressionSyntax equals)
            {
                if (Equality.IsObjectEquals(equals, semanticModel, cancellationToken, value, member) ||
                    Equality.IsInstanceEquals(equals, semanticModel, cancellationToken, value, member) ||
                    Equality.IsInstanceEquals(equals, semanticModel, cancellationToken, member, value) ||
                    Equality.IsReferenceEquals(equals, semanticModel, cancellationToken, value, member) ||
                    Equality.IsEqualityComparerEquals(equals, semanticModel, cancellationToken, value, member) ||
                    Equality.IsStringEquals(equals, semanticModel, cancellationToken, value, member) ||
                    Equality.IsNullableEquals(equals, semanticModel, cancellationToken, value, member))
                {
                    return true;
                }

                return false;
            }

            return Equality.IsOperatorEquals(expression, semanticModel, cancellationToken, value, member);
        }

        private static bool UsesValueAndMember(IfStatementSyntax ifStatement, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
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
