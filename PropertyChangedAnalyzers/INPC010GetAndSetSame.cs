namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC010GetAndSetSame
    {
        public const string DiagnosticId = "INPC010";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The property sets a different field than it returns.",
            messageFormat: "The property sets a different field than it returns.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "The property sets a different field than it returns.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
