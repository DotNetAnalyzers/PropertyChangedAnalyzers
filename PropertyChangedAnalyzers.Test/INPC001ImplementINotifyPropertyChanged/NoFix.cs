namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChanged;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class NoFix
{
    private static readonly ClassDeclarationAnalyzer Analyzer = new();
    private static readonly ImplementINotifyPropertyChangedFix Fix = new();
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
