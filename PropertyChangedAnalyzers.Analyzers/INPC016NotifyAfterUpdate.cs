namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC016NotifyAfterUpdate
    {
        public const string DiagnosticId = "INPC016";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Notify after update.",
            messageFormat: "Notify after updating the backing field.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Notify after updating the backing field.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}