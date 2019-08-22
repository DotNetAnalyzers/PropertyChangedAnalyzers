namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class AssignmentAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.INPC014SetBackingFieldInConstructor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.SimpleAssignmentExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is AssignmentExpressionSyntax assignment &&
                ShouldSetBackingField(assignment, context, out var fieldAccess))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.INPC014SetBackingFieldInConstructor,
                        assignment.GetLocation(),
                        additionalLocations: new[] { fieldAccess.GetLocation() }));
            }
        }

        private static bool ShouldSetBackingField(AssignmentExpressionSyntax assignment, SyntaxNodeAnalysisContext context, out ExpressionSyntax fieldAccess)
        {
            fieldAccess = null;
            return context.ContainingSymbol is IMethodSymbol ctor &&
                   !ctor.IsStatic &&
                   ctor.MethodKind == MethodKind.Constructor &&
                   !assignment.TryFirstAncestor(out AnonymousFunctionExpressionSyntax _) &&
                   !assignment.TryFirstAncestor(out LocalFunctionStatementSyntax _) &&
                   Property.TryGetAssignedProperty(assignment, out var propertyDeclaration) &&
                   propertyDeclaration.TryGetSetter(out var setter) &&
                   !ThrowWalker.Throws(setter) &&
                   Setter.TryFindSingleMutation(setter, context.SemanticModel, context.CancellationToken, out fieldAccess) &&
                   !HasSideEffects(setter, context);
        }

        private static bool HasSideEffects(AccessorDeclarationSyntax setter, SyntaxNodeAnalysisContext context)
        {
            using (var walker = InvocationWalker.Borrow(setter))
            {
                foreach (var invocation in walker.Invocations)
                {
                    if (invocation.TryGetMethodName(out var name) &&
                        (name == nameof(Equals) ||
                         name == nameof(ReferenceEquals) ||
                         name == "nameof"))
                    {
                        continue;
                    }

                    if (TrySet.IsMatch(invocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No)
                    {
                        continue;
                    }

                    if (OnPropertyChanged.IsMatch(invocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No ||
                        PropertyChangedEvent.IsInvoke(invocation, context.SemanticModel, context.CancellationToken))
                    {
                        continue;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
