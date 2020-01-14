namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class NoFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ClassDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new ImplementINotifyPropertyChangedFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC001ImplementINotifyPropertyChanged);

        [Test]
        [Ignore("Not sure how we want this.")]
        public static void IgnoresWhenBaseIsMouseGesture()
        {
            var code = @"
namespace N
{
    using System.Windows.Input;

    public class CustomGesture : MouseGesture
    {
        ↓public int P { get; set; }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }
    }
}
