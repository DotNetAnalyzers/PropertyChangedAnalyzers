namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class CS0535
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0535");

            [Test]
            public static void WhenInterfaceAndUsingSealedAddUsings()
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
            public static void WhenInterfaceAndUsingSealedFullyQualified()
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
                RoslynAssert.CodeFix<ImplementINotifyPropertyChangedFix>(ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            }

            [Test]
            public static void WhenInterfaceOnlyWithUsingAddUsing()
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
            public static void WhenInterfaceOnlyWithUsingFullyQualified()
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
                RoslynAssert.CodeFix<ImplementINotifyPropertyChangedFix>(ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            }

            [Test]
            public static void WhenInterfaceOnlyWithUsingUnderscoreAddUsings()
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
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix<ImplementINotifyPropertyChangedFix>(ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Implement INotifyPropertyChanged and add usings.");
            }

            [Test]
            public static void WhenInterfaceOnlyWithUsingUnderscoreFullyQualified()
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
                RoslynAssert.CodeFix<ImplementINotifyPropertyChangedFix>(ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            }

            [Test]
            public static void WhenInterfaceOnlyAndUsingsAddUsing()
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
                RoslynAssert.CodeFix<ImplementINotifyPropertyChangedFix>(ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Implement INotifyPropertyChanged and add usings.");
            }

            [Test]
            public static void WhenInterfaceOnlyAndUsingsFullyQualified()
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
                RoslynAssert.CodeFix<ImplementINotifyPropertyChangedFix>(ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            }
        }
    }
}
