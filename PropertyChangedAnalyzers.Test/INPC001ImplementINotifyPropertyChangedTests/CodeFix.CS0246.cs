namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class CS0246
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0246");

            [Test]
            public static void WhenInterfaceOnlyAddUsings()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : INotifyPropertyChanged
    {
    }
}";

                var fixedCode = @"
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
                RoslynAssert.CodeFix<ImplementINotifyPropertyChangedFix>(ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Implement INotifyPropertyChanged and add usings.");
            }

            [Test]
            public static void WhenInterfaceOnlyFullyQualified()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : INotifyPropertyChanged
    {
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix<ImplementINotifyPropertyChangedFix>(ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            }

            [Test]
            public static void WhenInterfaceOnlySealedAddUsings()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public sealed class Foo : INotifyPropertyChanged
    {
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix<ImplementINotifyPropertyChangedFix>(ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Implement INotifyPropertyChanged and add usings.");
            }

            [Test]
            public static void WhenInterfaceOnlySealedFullyQualified()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public sealed class Foo : INotifyPropertyChanged
    {
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public sealed class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix<ImplementINotifyPropertyChangedFix>(ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            }
        }
    }
}
