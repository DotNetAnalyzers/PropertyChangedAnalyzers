namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly INPC008StructMustNotNotify Analyzer = new INPC008StructMustNotNotify();

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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
