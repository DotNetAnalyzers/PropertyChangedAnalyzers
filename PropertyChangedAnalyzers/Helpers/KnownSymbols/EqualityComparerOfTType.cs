namespace PropertyChangedAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;

internal class EqualityComparerOfTType : QualifiedType
{
    internal readonly QualifiedMethod EqualsMethod;

    internal EqualityComparerOfTType()
        : base("System.Collections.Generic.EqualityComparer`1")
    {
        this.EqualsMethod = new QualifiedMethod(this, "Equals");
    }
}
