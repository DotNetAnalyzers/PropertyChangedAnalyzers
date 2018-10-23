namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC014SetBackingField : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC014";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Prefer setting backing field in constructor.",
            messageFormat: "Prefer setting backing field in constructor.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Prefer setting backing field in constructor.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

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
                !context.ContainingSymbol.IsStatic &&
                IsInConstructor(assignment) &&
                Property.TryGetAssignedProperty(assignment, out var propertyDeclaration) &&
                propertyDeclaration.TryGetSetter(out var setter) &&
                (setter.Body != null || setter.ExpressionBody != null) &&
                !ThrowWalker.Throws(setter) &&
                IsAssignedWithParameter(setter, context) &&
                !HasSideEffects(setter, context))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignment.GetLocation()));
            }
        }

        private static bool IsInConstructor(SyntaxNode node)
        {
            if (node.FirstAncestor<ConstructorDeclarationSyntax>() == null)
            {
                return false;
            }

            // Could be in an event handler in ctor.
            return node.FirstAncestor<AnonymousFunctionExpressionSyntax>() == null;
        }

        private static bool IsAssignedWithParameter(AccessorDeclarationSyntax setter, SyntaxNodeAnalysisContext context)
        {
            using (var mutations = MutationWalker.Borrow(setter, Scope.Member, context.SemanticModel, context.CancellationToken))
            {
                if (mutations.TrySingle(out var mutation))
                {
                    switch (mutation)
                    {
                        case AssignmentExpressionSyntax assignment:
                            return assignment.Right is IdentifierNameSyntax identifierName &&
                                   identifierName.Identifier.ValueText == "value";
                        case ArgumentSyntax argument:
                            return argument.Parent is ArgumentListSyntax argumentList &&
                                   argumentList.Parent is InvocationExpressionSyntax invocation &&
                                   PropertyChanged.IsSetAndRaiseCall(invocation, context.SemanticModel, context.CancellationToken) == AnalysisResult.Yes;
                        default:
                            return false;
                    }
                }
            }

            return false;
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

                    if (PropertyChanged.IsSetAndRaiseCall(invocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No)
                    {
                        continue;
                    }

                    if (PropertyChanged.IsOnPropertyChanged(invocation, context.SemanticModel, context.CancellationToken) != AnalysisResult.No ||
                        PropertyChanged.IsPropertyChangedInvoke(invocation, context.SemanticModel, context.CancellationToken))
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
