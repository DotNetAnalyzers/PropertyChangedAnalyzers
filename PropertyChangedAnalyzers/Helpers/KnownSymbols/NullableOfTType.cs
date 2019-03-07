namespace PropertyChangedAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class NullableOfTType : QualifiedType
    {
        internal NullableOfTType()
            : base("System.Nullable`1")
        {
        }
    }
}
