namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotify;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly StructAnalyzer Analyzer = new();

    [Test]
    public static void SimpleStruct()
    {
        var code = @"
namespace N
{
    public struct S
    {
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }
}
