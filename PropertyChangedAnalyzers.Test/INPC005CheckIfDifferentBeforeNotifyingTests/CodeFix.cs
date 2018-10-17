namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;

    internal partial class CodeFix
    {
        private static readonly InvocationAnalyzer Analyzer = new InvocationAnalyzer();
        private static readonly CheckIfDifferentBeforeNotifyFix Fix = new CheckIfDifferentBeforeNotifyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("INPC005");
    }
}
