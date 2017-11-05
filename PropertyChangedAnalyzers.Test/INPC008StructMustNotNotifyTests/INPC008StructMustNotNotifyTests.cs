namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly INPC008StructMustNotNotify Analyzer = new INPC008StructMustNotNotify();

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
            AnalyzerAssert.Diagnostics(Analyzer, testCode);
        }
    }
}
