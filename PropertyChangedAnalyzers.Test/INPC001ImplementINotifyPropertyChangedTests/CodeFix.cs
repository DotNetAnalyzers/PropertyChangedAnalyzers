namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        [Test]
        public void WhenNotNotifyingAutoProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        ↓public int Bar { get; set; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Bar { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void WhenNotNotifyingWithBackingField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int value;

        ↓public int Value
        {
            get
            {
                return this.value;
            }
            private set
            {
                this.value = value;
            }
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private int value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return this.value;
            }
            private set
            {
                this.value = value;
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void WhenNotNotifyingWithBackingFieldUnderscoreNames()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int _value;

        ↓public int Value
        {
            get
            {
                return _value;
            }
            private set
            {
                _value = value;
            }
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        private int _value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return _value;
            }
            private set
            {
                _value = value;
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void WhenInterfaceOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo : ↓INotifyPropertyChanged
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
            AnalyzerAssert.CodeFix<ImplementINotifyPropertyChangedCodeFixProvider>("CS0246", testCode, fixedCode);
        }

        [Test]
        public void WhenInterfaceOnlySealed()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public sealed class Foo : ↓INotifyPropertyChanged
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
            AnalyzerAssert.CodeFix<ImplementINotifyPropertyChangedCodeFixProvider>("CS0246", testCode, fixedCode);
        }

        [Test]
        public void WhenInterfaceAndUsingSealed()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class Foo : ↓INotifyPropertyChanged
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
            AnalyzerAssert.CodeFix<ImplementINotifyPropertyChangedCodeFixProvider>("CS0535", testCode, fixedCode);
        }

        [Test]
        public void WhenInterfaceOnlyWithUsing()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : ↓INotifyPropertyChanged
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
            AnalyzerAssert.CodeFix<ImplementINotifyPropertyChangedCodeFixProvider>("CS0535", testCode, fixedCode);
        }

        [Test]
        public void WhenInterfaceOnlyWithUsingUnderscore()
        {
            var testCode = @"
#pragma warning disable 169
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : ↓INotifyPropertyChanged
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
            AnalyzerAssert.CodeFix<ImplementINotifyPropertyChangedCodeFixProvider>("CS0535", testCode, fixedCode);
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

    public class Foo : ↓INotifyPropertyChanged
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
            AnalyzerAssert.CodeFix<ImplementINotifyPropertyChangedCodeFixProvider>("CS0535", testCode, fixedCode);
        }

        [Test]
        public void WhenEventOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;
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
            AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void WhenEventAndInvokerOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo
    {
        ↓public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
            AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }
    }
}