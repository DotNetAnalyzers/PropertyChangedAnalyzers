namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal class INPC006UseReferenceEqualsForReferenceTypes
    {
        public const string DiagnosticId = "INPC006_a";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Check if value is different using ReferenceEquals before notifying.",
            messageFormat: "Check if value is different using ReferenceEquals before notifying.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Check if value is different using ReferenceEquals before notifying.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
