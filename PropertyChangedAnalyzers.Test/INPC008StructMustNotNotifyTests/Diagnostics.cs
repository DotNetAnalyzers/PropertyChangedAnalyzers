namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotifyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new INPC008StructMustNotNotify();

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
            RoslynAssert.Diagnostics(Analyzer, testCode);
        }

        [Test]
        public void WhenNotifyingFullyQualified()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public struct Foo : ↓System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}";
            RoslynAssert.Diagnostics(Analyzer, testCode);
        }
    }
}
