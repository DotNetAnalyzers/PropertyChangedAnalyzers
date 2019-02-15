namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC022NotifiesTwice
    {
        public const string DiagnosticId = "INPC022";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The change is already notified for.",
            messageFormat: "The change is already notified for.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "The change is already notified for.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
