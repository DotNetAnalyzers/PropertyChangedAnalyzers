namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC018InvokerShouldBeProtected
    {
        public const string DiagnosticId = "INPC018";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "PropertyChanged invoker should be protected when the class is not sealed.",
            messageFormat: "PropertyChanged invoker should be protected when the class is not sealed.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "PropertyChanged invoker should be protected when the class is not sealed.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}