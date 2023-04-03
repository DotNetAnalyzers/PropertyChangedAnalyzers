namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class NoFix
{
    private static readonly SetAccessorAnalyzer Analyzer = new();
    private static readonly MakePropertyNotifyFix Fix = new();
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
        public int ↓P { get; set; }
    }
}";

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
    }

    [Test]
    public static void AutoPropertyExplicitNameHandlesRecursionInInvoker()
    {
        var code = @"
#pragma warning disable CS0067
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(propertyName);
        }
    }
}";

        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
    }
}
