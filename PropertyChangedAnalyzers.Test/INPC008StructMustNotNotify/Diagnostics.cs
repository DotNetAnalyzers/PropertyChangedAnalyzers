namespace PropertyChangedAnalyzers.Test.INPC008StructMustNotNotify
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly StructAnalyzer Analyzer = new();

        [Test]
        public static void WhenNotifying()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public struct S : ↓INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}";
            RoslynAssert.Diagnostics(Analyzer, code);
        }

        [Test]
        public static void WhenNotifyingFullyQualified()
        {
            var code = @"
namespace N
{
    public struct S : ↓System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }
}";
            RoslynAssert.Diagnostics(Analyzer, code);
        }
    }
}
