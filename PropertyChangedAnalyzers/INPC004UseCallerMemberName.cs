namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC004UseCallerMemberName
    {
        public const string DiagnosticId = "INPC004";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use [CallerMemberName]",
            messageFormat: "Use [CallerMemberName]",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Use [CallerMemberName]",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
