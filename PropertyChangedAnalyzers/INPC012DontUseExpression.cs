namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC012DontUseExpression
    {
        public const string DiagnosticId = "INPC012";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't use expression for raising PropertyChanged.",
            messageFormat: "Don't use expression for raising PropertyChanged.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Don't use expression for raising PropertyChanged.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
