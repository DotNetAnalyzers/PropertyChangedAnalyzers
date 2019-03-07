namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC021SetBackingField
    {
        public const string DiagnosticId = "INPC021";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Setter should set backing field.",
            messageFormat: "Setter should set backing field.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Setter should set backing field.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}