namespace PropertyChangedAnalyzers.Test.INPC011DoNotShadowTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new EventAnalyzer();
        private static readonly CodeFixProvider Fix = new RemoveShadowingFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC011DoNotShadow);

        [Test]
        public static void ShadowingEvent()
        {
            var viewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var before = @"
namespace RoslynSandbox.Client
{
    using System.ComponentModel;

    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var after = @"
namespace RoslynSandbox.Client
{
    using System.ComponentModel;

    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after);
        }
    }
}
