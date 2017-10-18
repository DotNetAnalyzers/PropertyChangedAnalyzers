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

            if (Property.TryGetSingleAssignmentInSetter(setter, out _) &&
                Property.TryFindValue(setter, context.SemanticModel, context.CancellationToken, out var value))
            {
                using (var walker = IfStatementWalker.Borrow(setter))
                {
                    foreach (var ifStatement in walker.IfStatements)
                    {
                        if (ifStatement.SpanStart >= invocation.SpanStart)
                        {
                            continue;
                        }

                        foreach (var member in new ISymbol[] { backingField, property })
                        {
                            if (Equality.IsOperatorEquals(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member) ||
                                IsEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member))
                            {
                                if (ifStatement.Statement.Span.Contains(invocation.Span))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                                }

                                return;
                            }

                            if (Equality.IsOperatorNotEquals(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member) ||
                                IsNegatedEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, member))
                            {
                                if (!ifStatement.Statement.Span.Contains(invocation.Span))
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
                                }

                                return;
                            }

                            if (UsesValueAndMember(ifStatement, context.SemanticModel, context.CancellationToken, value, member))
                            {
                                return;
                            }
                        }
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.FirstAncestorOrSelf<StatementSyntax>()?.GetLocation() ?? invocation.GetLocation()));
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
            var unaryExpression = expression as PrefixUnaryExpressionSyntax;
            if (unaryExpression?.IsKind(SyntaxKind.LogicalNotExpression) == true)
            {
                return IsEqualsCheck(unaryExpression.Operand, semanticModel, cancellationToken, value, member);
            }

            return false;
        }

        private static bool IsEqualsCheck(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            var equals = expression as InvocationExpressionSyntax;
            if (equals == null)
            {
                return false;
            }

            if (Equality.IsObjectEquals(equals, semanticModel, cancellationToken, value, member) ||
                Equality.IsInstanceEquals(equals, semanticModel, cancellationToken, value, member) ||
                Equality.IsInstanceEquals(equals, semanticModel, cancellationToken, member, value) ||
                Equality.IsReferenceEquals(equals, semanticModel, cancellationToken, value, member) ||
                Equality.IsEqualityComparerEquals(equals, semanticModel, cancellationToken, value, member) ||
                Equality.IsNullableEquals(equals, semanticModel, cancellationToken, value, member))
            {
                return true;
            }

            return false;
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