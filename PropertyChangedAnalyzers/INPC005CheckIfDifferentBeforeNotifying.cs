namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC005CheckIfDifferentBeforeNotifying
    {
        public const string DiagnosticId = "INPC005";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Check if value is different before notifying.",
            messageFormat: "Check if value is different before notifying.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Check if value is different before notifying.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
