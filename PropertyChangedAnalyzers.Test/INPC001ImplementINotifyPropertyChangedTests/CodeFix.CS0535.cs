namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class CS0535
        {
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0535");

            [Test]
            public void WhenInterfaceAndUsingSealed()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class Foo : INotifyPropertyChanged
    {
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementINotifyPropertyChangedCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void WhenInterfaceOnlyWithUsing()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementINotifyPropertyChangedCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void WhenInterfaceOnlyWithUsingUnderscore()
            {
                var testCode = @"
#pragma warning disable 169
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        private int _value;
    }
}";

                var fixedCode = @"
#pragma warning disable 169
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementINotifyPropertyChangedCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void WhenInterfaceOnlyAndUsings()
            {
                var testCode = @"
#pragma warning disable 8019
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
    }
}";

                var fixedCode = @"
#pragma warning disable 8019
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix<ImplementINotifyPropertyChangedCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
            }
        }
    }
}