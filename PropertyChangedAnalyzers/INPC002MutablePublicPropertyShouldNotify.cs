namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC002MutablePublicPropertyShouldNotify
    {
        public const string DiagnosticId = "INPC002";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Mutable public property should notify.",
            messageFormat: "Property '{0}' should notify when value changes.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "All mutable public properties should notify when their value changes.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
