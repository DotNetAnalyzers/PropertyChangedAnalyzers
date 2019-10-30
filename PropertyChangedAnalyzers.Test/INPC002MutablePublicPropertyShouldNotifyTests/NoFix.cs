namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class NoFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetAccessorAnalyzer();
        private static readonly CodeFixProvider Fix = new MakePropertyNotifyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC002MutablePublicPropertyShouldNotify);

        [Test]
        [Ignore("Not sure how we want this.")]
        public static void NoFixWhenBaseHasInternalOnPropertyChanged()
        {
            var before = @"
namespace RoslynSandBox
{
    using System.Windows.Input;

    public class CustomGesture : MouseGesture
    {
        public int ↓Foo { get; set; }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, before);
        }
    }
}
