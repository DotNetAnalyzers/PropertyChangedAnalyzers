namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifying
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;

    public static partial class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetAccessorAnalyzer();
        private static readonly CodeFixProvider Fix = new CheckIfDifferentBeforeNotifyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC005CheckIfDifferentBeforeNotifying);
    }
}
