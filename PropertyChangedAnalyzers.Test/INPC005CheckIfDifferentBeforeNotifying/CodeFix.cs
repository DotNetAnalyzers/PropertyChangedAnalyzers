namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifying;

using Gu.Roslyn.Asserts;

public static partial class CodeFix
{
    private static readonly SetAccessorAnalyzer Analyzer = new();
    private static readonly CheckIfDifferentBeforeNotifyFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC005CheckIfDifferentBeforeNotifying);
}
