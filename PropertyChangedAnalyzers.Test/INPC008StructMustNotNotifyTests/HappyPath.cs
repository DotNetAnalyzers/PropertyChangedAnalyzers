namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public class HappyPath
    {
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
            AnalyzerAssert.Valid<INPC008StructMustNotNotify>(testCode);
        }
    }
}