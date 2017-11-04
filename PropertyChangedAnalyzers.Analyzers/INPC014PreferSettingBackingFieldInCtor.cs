namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC014PreferSettingBackingFieldInCtor : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC014";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
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
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.SimpleAssignmentExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is AssignmentExpressionSyntax assignment &&
                !context.ContainingSymbol.IsStatic &&
                IsInConstructor(assignment) &&
                Property.TryGetAssignedProperty(assignment, out var propertyDeclaration) &&
                propertyDeclaration.TryGetSetAccessorDeclaration(out var setter) &&
                Property.TryGetSingleAssignmentInSetter(setter, out _))
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
    }
}