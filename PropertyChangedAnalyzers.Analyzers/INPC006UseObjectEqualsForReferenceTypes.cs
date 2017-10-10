namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC006UseObjectEqualsForReferenceTypes : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC006_b";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Check if value is different using object.Equals before notifying.",
            messageFormat: "Check if value is different using object.Equals before notifying.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: AnalyzerConstants.DisabledByDefault,
            description: "Check if value is different using object.Equals before notifying.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.IfStatement);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var ifStatement = (IfStatementSyntax)context.Node;
            if (ifStatement?.Condition == null)
            {
                return;
            }

            var setter = ifStatement.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
            if (setter?.IsKind(SyntaxKind.SetAccessorDeclaration) != true)
            {
                return;
            }

            if (!Notifies(setter, context.SemanticModel, context.CancellationToken))
            {
                return;
            }

            var propertyDeclaration = setter.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var property = context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);

            if (property == null ||
                property.Type.IsValueType ||
                property.Type == KnownSymbol.String)
            {
                return;
            }

            if (!Property.TryGetBackingFieldAssignedInSetter(property, context.SemanticModel, context.CancellationToken, out var backingField))
            {
                return;
            }

            if (Property.TryFindValue(setter, context.SemanticModel, context.CancellationToken, out var value))
            {
                if (IsObjectEqualsOrNegated(ifStatement, context.SemanticModel, context.CancellationToken, value, backingField) ||
                    IsObjectEqualsOrNegated(ifStatement, context.SemanticModel, context.CancellationToken, value, property) ||
                    IsEqualityComparerEqualsOrNegated(ifStatement, context.SemanticModel, context.CancellationToken, value, backingField) ||
                    IsEqualityComparerEqualsOrNegated(ifStatement, context.SemanticModel, context.CancellationToken, value, property))
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, ifStatement.GetLocation()));
            }
        }

        private static bool Notifies(AccessorDeclarationSyntax setter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using (var pooled = InvocationWalker.Create(setter))
            {
                foreach (var invocation in pooled.Item.Invocations)
                {
                    if (PropertyChanged.IsNotifyPropertyChanged(invocation, semanticModel, cancellationToken))
                    {
                        return true;
                    }
                }
            }

            return false;
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
    }
}