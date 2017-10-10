namespace PropertyChangedAnalyzers.Test.INPC007MissingInvokerTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        [Test]
        public void EventOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC007MissingInvoker, AddInvokerCodeFix>(testCode, fixedCode);
        }

        [Test]
        public void EventOnlyWithUsing()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC007MissingInvoker, AddInvokerCodeFix>(testCode, fixedCode);
        }

        [Test]
        public void EventOnlySealed()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class ViewModel : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC007MissingInvoker, AddInvokerCodeFix>(testCode, fixedCode);
        }
    }
}