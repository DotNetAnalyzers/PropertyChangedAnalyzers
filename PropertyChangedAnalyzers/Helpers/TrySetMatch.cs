namespace PropertyChangedAnalyzers;

using System.Diagnostics.CodeAnalysis;

internal readonly struct TrySetMatch<T>
{
    internal readonly AnalysisResult AnalysisResult;

    internal readonly T Field;

    internal readonly T Value;

    [AllowNull]
    internal readonly T Name;

    internal TrySetMatch(AnalysisResult analysisResult, T field, T value, [AllowNull]T name)
    {
        this.AnalysisResult = analysisResult;
        this.Field = field;
        this.Value = value;
        this.Name = name;
    }
}
