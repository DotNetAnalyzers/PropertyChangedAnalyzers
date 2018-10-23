namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC014SetBackingField
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
    }
}
