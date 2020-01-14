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
            var code = @"
namespace N
{
    using System.Windows.Input;

    public class CustomGesture : MouseGesture
    {
        public int ↓Foo { get; set; }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }

        [Test]
        public static void AutoPropertyExplicitNameHandlesRecursionInInvoker()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(propertyName);
        }
    }
}";

            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
        }
    }
}
