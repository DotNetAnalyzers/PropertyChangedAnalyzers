namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC017BackingFieldNameMustMatch
    {
        public const string DiagnosticId = "INPC017";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field name must match.",
            messageFormat: "Backing field name must match.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Backing field name must match.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
