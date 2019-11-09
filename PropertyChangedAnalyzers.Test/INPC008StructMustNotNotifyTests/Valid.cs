namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotifyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new StructAnalyzer();

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
}
