namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotifyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new INPC008StructMustNotNotify();

        [Test]
        public static void WhenNotifying()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public struct Foo : ↓INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}";
            RoslynAssert.Diagnostics(Analyzer, code);
        }

        [Test]
        public static void WhenNotifyingFullyQualified()
        {
            var code = @"
namespace RoslynSandbox
{
    public struct Foo : ↓System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}";
            RoslynAssert.Diagnostics(Analyzer, code);
        }
    }
}
