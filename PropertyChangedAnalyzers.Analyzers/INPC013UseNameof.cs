namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class INPC013UseNameof : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INPC013";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use nameof.",
            messageFormat: "Use nameof.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Use nameof.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.Argument);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is ArgumentSyntax argument &&
                argument.Expression is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                SyntaxFacts.IsValidIdentifier(literal.Token.ValueText))
            {
                if (context.ContainingSymbol is IMethodSymbol method &&
                    method.Parameters.TryGetSingle(x => x.Name == literal.Token.ValueText, out _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation()));
                }

                if (context.ContainingSymbol.ContainingType.TryGetProperty(literal.Token.ValueText, out _))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, argument.GetLocation()));
                }
            }
        }
    }
}