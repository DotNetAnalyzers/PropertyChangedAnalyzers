namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC006UseObjectEqualsForReferenceTypes
    {
        public const string DiagnosticId = "INPC006_b";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Check if value is different using object.Equals before notifying.",
            messageFormat: "Check if value is different using object.Equals before notifying.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: AnalyzerConstants.DisabledByDefault,
            description: "Check if value is different using object.Equals before notifying.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
