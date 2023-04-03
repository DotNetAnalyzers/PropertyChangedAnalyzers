namespace PropertyChangedAnalyzers.Test.INPC011DoNotShadow;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly EventAnalyzer Analyzer = new();
    private static readonly RemoveShadowingFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC011DoNotShadow);

    [Test]
    public static void ShadowingEvent()
    {
        var viewModelBaseCode = @"
namespace N.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        var before = @"
namespace N.Client
{
    using System.ComponentModel;

    public class C : N.Core.ViewModelBase
    {
        ↓public event PropertyChangedEventHandler? PropertyChanged;
    }
}";

        var after = @"
namespace N.Client
{
    using System.ComponentModel;

    public class C : N.Core.ViewModelBase
    {
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after);
    }
}
