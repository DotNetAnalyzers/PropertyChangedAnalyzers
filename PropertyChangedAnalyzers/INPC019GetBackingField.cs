namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class INPC019GetBackingField
    {
        public const string DiagnosticId = "INPC019";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Getter should return backing field.",
            messageFormat: "Getter should return backing field.",
            category: AnalyzerCategory.PropertyChanged,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Getter should return backing field.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
