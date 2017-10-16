namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        [Test]
        public void WhenAutoPropertiesMessage()
        {
            var testCode = @"
namespace RoslynSandbox
{
    ↓public class Foo
    {
        public int Bar1 { get; set; }

        public int Bar2 { get; set; }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Bar1 { get; set; }

        public int Bar2 { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var expectedMessage = ExpectedMessage.Create("The class Foo should notify for:\nBar1\nBar2");
            AnalyzerAssert.Diagnostics<INPC001ImplementINotifyPropertyChanged>(expectedMessage, testCode);
            AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
            AnalyzerAssert.FixAll<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void WhenNotNotifyingAutoProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    ↓public class Foo
    {
        public int Bar { get; set; }
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
            var expectedMessage = ExpectedMessage.Create("The class Foo should notify for:\nBar");
            AnalyzerAssert.Diagnostics<INPC001ImplementINotifyPropertyChanged>(expectedMessage, testCode);
            AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
            AnalyzerAssert.FixAll<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void WhenNotNotifyingWithBackingField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    ↓public class Foo
    {
        private int value;

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
            AnalyzerAssert.FixAll<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void WhenNotNotifyingWithBackingFieldExpressionBodies()
        {
            var testCode = @"
namespace RoslynSandbox
{
    ↓public class Foo
    {
        private int value;

        public int Value
        {
            get => this.value;
            private set =>this.value = value;
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
            get => this.value;
            private set =>this.value = value;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
            AnalyzerAssert.FixAll<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void WhenNotNotifyingWithBackingFieldUnderscoreNames()
        {
            var testCode = @"
namespace RoslynSandbox
{
    ↓public class Foo
    {
        private int _value;

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
            AnalyzerAssert.FixAll<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void WhenEventOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    ↓public class Foo
    {
        public event PropertyChangedEventHandler PropertyChanged;
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
            var expectedMessage = ExpectedMessage.Create("The class Foo has event PropertyChanged but does not implement INotifyPropertyChanged.");
            AnalyzerAssert.Diagnostics<INPC001ImplementINotifyPropertyChanged>(expectedMessage, testCode);
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

    ↓public class Foo
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        [Test]
        [Explicit("Not sure how we want this.")]
        public void IgnoresWhenBaseIsMouseGesture()
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System.Windows.Input;

    public class CustomGesture : MouseGesture
    {
        ↓public int Foo { get; set; }
    }
}";

            AnalyzerAssert.NoFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode);
        }
    }
}