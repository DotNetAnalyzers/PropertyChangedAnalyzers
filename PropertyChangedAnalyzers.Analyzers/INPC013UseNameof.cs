namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC013UseNameof
    {
        public const string DiagnosticId = "INPC013";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use nameof.",
            messageFormat: "Use nameof.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Use nameof.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
