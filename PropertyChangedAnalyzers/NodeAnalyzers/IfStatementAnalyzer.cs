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
    internal class IfStatementAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            INPC006UseReferenceEqualsForReferenceTypes.Descriptor,
            INPC006UseObjectEqualsForReferenceTypes.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.IfStatement);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is IfStatementSyntax ifStatement &&
                ifStatement.Condition != null)
            {
                if (ifStatement.FirstAncestorOrSelf<AccessorDeclarationSyntax>() is AccessorDeclarationSyntax setter &&
                    setter.IsKind(SyntaxKind.SetAccessorDeclaration) &&
                    context.ContainingSymbol is IMethodSymbol setMethod &&
                    setMethod.Parameters.TrySingle(out var value) &&
                    Property.TryGetContainingProperty(context.ContainingSymbol, out var property) &&
                    Property.TrySingleAssignmentInSetter(setter, out var assignment) &&
                    context.SemanticModel.GetSymbolSafe(assignment.Left, context.CancellationToken) is ISymbol backingField)
                {
                    if (property.Type.IsReferenceType &&
                        property.Type != KnownSymbol.String)
                    {
                        if (INPC006UseReferenceEqualsForReferenceTypes.Descriptor.IsSuppressed(context.SemanticModel) &&
                            !IsObjectEqualsOrNegated(ifStatement, context.SemanticModel, context.CancellationToken, value, backingField) &&
                            !IsObjectEqualsOrNegated(ifStatement, context.SemanticModel, context.CancellationToken, value, property) &&
                            !IsEqualityComparerEqualsOrNegated(ifStatement, context.SemanticModel, context.CancellationToken, value, backingField) &&
                            !IsEqualityComparerEqualsOrNegated(ifStatement, context.SemanticModel, context.CancellationToken, value, property))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC006UseObjectEqualsForReferenceTypes.Descriptor, ifStatement.GetLocation()));
                        }

                        if (INPC006UseObjectEqualsForReferenceTypes.Descriptor.IsSuppressed(context.SemanticModel) &&
                            !Equality.IsReferenceEquals(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, backingField) &&
                            !Equality.IsReferenceEquals(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, property) &&
                            !IsNegatedReferenceEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, backingField) &&
                            !IsNegatedReferenceEqualsCheck(ifStatement.Condition, context.SemanticModel, context.CancellationToken, value, property))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(INPC006UseReferenceEqualsForReferenceTypes.Descriptor, ifStatement.GetLocation()));
                        }
                    }
                }
            }
        }

        private static bool IsObjectEqualsOrNegated(IfStatementSyntax ifStatement, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            if (Equality.IsObjectEquals(ifStatement.Condition, semanticModel, cancellationToken, value, member))
            {
                return Equality.UsesObjectOrNone(ifStatement.Condition);
            }

            if (ifStatement.Condition is PrefixUnaryExpressionSyntax unary &&
                unary.IsKind(SyntaxKind.LogicalNotExpression))
            {
                return Equality.IsObjectEquals(unary.Operand, semanticModel, cancellationToken, value, member) &&
                       Equality.UsesObjectOrNone(ifStatement.Condition);
            }

            return false;
        }

        private static bool IsEqualityComparerEqualsOrNegated(IfStatementSyntax ifStatement, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            if (Equality.IsEqualityComparerEquals(ifStatement.Condition, semanticModel, cancellationToken, value, member))
            {
                return true;
            }

            if (ifStatement.Condition is PrefixUnaryExpressionSyntax unary &&
                unary.IsKind(SyntaxKind.LogicalNotExpression))
            {
                return Equality.IsEqualityComparerEquals(unary.Operand, semanticModel, cancellationToken, value, member);
            }

            return false;
        }

        private static bool IsNegatedReferenceEqualsCheck(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, ISymbol member)
        {
            var unaryExpression = expression as PrefixUnaryExpressionSyntax;
            if (unaryExpression?.IsKind(SyntaxKind.LogicalNotExpression) == true)
            {
                return Equality.IsReferenceEquals(unaryExpression.Operand, semanticModel, cancellationToken, value, member);
            }

            return false;
        }
    }
}
