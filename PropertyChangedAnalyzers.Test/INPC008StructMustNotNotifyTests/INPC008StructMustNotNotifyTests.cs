namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public class Diagnostics
    {
        [Test]
        public void WhenNotifying()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public struct Foo : ↓INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}";
            AnalyzerAssert.Diagnostics<INPC008StructMustNotNotify>(testCode);
        }
    }
}
