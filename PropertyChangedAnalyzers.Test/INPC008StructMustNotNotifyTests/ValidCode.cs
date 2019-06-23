namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotifyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new INPC008StructMustNotNotify();

        [Test]
        public void SimpleStruct()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public struct Foo
    {
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
