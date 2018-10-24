namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC020PreferExpressionBodyAccessor
    {
        public const string DiagnosticId = "INPC020";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Prefer expression body accessor.",
            messageFormat: "Prefer expression body accessor.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Prefer expression body accessor.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}