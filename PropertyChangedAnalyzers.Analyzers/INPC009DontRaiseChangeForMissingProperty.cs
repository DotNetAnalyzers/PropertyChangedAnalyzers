namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC009DontRaiseChangeForMissingProperty
    {
        public const string DiagnosticId = "INPC009";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't raise PropertyChanged for missing property.",
            messageFormat: "Don't raise PropertyChanged for missing property.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Don't raise PropertyChanged for missing property.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
