namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotifyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new INPC008StructMustNotNotify();

        [Test]
        public static void SimpleStruct()
        {
            var code = @"
namespace RoslynSandbox
{
    public struct Foo
    {
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
