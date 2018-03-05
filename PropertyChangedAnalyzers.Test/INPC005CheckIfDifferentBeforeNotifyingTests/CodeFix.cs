namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;

    internal partial class Codefix
    {
        private static readonly InvocationAnalyzer Analyzer = new InvocationAnalyzer();
        private static readonly CheckIfDifferentBeforeNotifyFixProvider CodeFix = new CheckIfDifferentBeforeNotifyFixProvider();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("INPC005");
    }
}
