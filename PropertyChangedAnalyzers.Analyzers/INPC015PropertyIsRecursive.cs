namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC015PropertyIsRecursive
    {
        public const string DiagnosticId = "INPC015";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Property is recursive.",
            messageFormat: "Property is recursive {0}.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Property is recursive.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}