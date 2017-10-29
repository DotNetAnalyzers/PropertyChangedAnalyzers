namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC005CheckIfDifferentBeforeNotifying : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC005";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Check if value is different before notifying.",
            messageFormat: "Check if value is different before notifying.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Check if value is different before notifying.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var invocation = (InvocationExpressionSyntax)context.Node;
            var setter = invocation.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
            if (setter?.IsKind(SyntaxKind.SetAccessorDeclaration) != true)
            {
                return;
            }

            if (!IsFirstNotifyPropertyChange(invocation, context.SemanticModel, context.CancellationToken))
            {
                return;
            }

            var propertyDeclaration = setter.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var property = context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);
            if (!Property.TryGetBackingFieldFromSetter(property, context.SemanticModel, context.CancellationToken, out var backingField))
            {
                return;
            }

            if (Property.TryGetSingleAssignmentInSetter(setter, out var assignment) &&
                Property.TryFindValue(setter, context.SemanticModel, context.CancellationToken, out var value))
            {
                if (!AreInSameBlock(assignment, invocation) ||
                    assignment.SpanStart > invocation.SpanStart)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                    return;
                }

                using (var walker = IfStatementWalker.Borrow(setter))
                {
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
                                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                            }

                            return;
                        }

                        if (IsNegatedEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, backingField) ||
                            IsNegatedEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, property))
                        {
                            if (!ifStatement.Statement.Span.Contains(invocation.Span) ||
                                ifStatement.IsReturnIfTrue())
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                            }

                            return;
                        }
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                }
            }
            else if (Property.TryGetSingleSetAndRaiseInSetter(setter, context.SemanticModel, context.CancellationToken, out var setAndRaise))
            {
                if (setAndRaise.Parent is IfStatementSyntax ifStatement1 &&
                    ifStatement1.Span.Contains(invocation.Span))
                {
                    return;
                }

                if (setAndRaise.Parent is PrefixUnaryExpressionSyntax unary &&
                    unary.IsKind(SyntaxKind.LogicalNotExpression) &&
                    unary.Parent is IfStatementSyntax ifStatement2 &&
                    !ifStatement2.Span.Contains(invocation.Span))
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
            }
        }

        private static bool AreInSameBlock(SyntaxNode node1, SyntaxNode node2)
        {
            if (node1?.FirstAncestor<BlockSyntax>() is BlockSyntax block1 &&
                node2?.FirstAncestor<BlockSyntax>() is BlockSyntax block2)
            {
                return block1 == block2;
            }

            return false;
        }

        private static bool IsFirstNotifyPropertyChange(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (!PropertyChanged.IsNotifyPropertyChanged(invocation, semanticModel, cancellationToken))
            {
                return false;
            }

            var statement = invocation.FirstAncestorOrSelf<ExpressionStatementSyntax>();
            var block = statement?.FirstAncestorOrSelf<BlockSyntax>();

            if (block == null)
            {
                return false;
            }

            var index = block.Statements.IndexOf(statement);
            if (index <= 0)
            {
                return false;
            }

            return !PropertyChanged.IsNotifyPropertyChanged(block.Statements[index - 1], semanticModel, cancellationToken);
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
    }
}