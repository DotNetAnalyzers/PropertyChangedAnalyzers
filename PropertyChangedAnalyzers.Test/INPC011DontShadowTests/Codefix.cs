namespace PropertyChangedAnalyzers.Test.INPC011DontShadowTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Codefix
    {
        [Test]
        public void ShadowingEvent()
        {
            var viewModelBaseCode = @"
namespace RoslynSandbox
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

            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : ViewModelBase
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : ViewModelBase
    {
    }
}";

            AnalyzerAssert.CodeFix<INPC011DontShadow, RemoveShadowingCodeFix>(new[] { viewModelBaseCode, testCode }, fixedCode);
        }
    }
}