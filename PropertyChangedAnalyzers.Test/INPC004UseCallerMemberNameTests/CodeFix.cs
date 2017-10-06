namespace PropertyChangedAnalyzers.Test.INPC004UseCallerMemberNameTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        [TestCase(@"""Value""")]
        [TestCase(@"nameof(Value)")]
        [TestCase(@"nameof(this.Value)")]
        public void CallsOnPropertyChangedWithExplicitNameOfCaller(string propertyName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged(↓nameof(Value));
            }
        }

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

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            testCode = testCode.AssertReplace(@"nameof(Value)", propertyName);
            AnalyzerAssert.CodeFix<INPC004UseCallerMemberName, UseCallerMemberNameCodeFixProvider>(testCode, fixedCode);
            AnalyzerAssert.FixAll<INPC004UseCallerMemberName, UseCallerMemberNameCodeFixProvider>(testCode, fixedCode);
        }

        [TestCase("this.PropertyChanged")]
        [TestCase("PropertyChanged")]
        public void Invoker(string member)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(↓string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
            testCode = testCode.AssertReplace("this.PropertyChanged", member);
            fixedCode = fixedCode.AssertReplace("this.PropertyChanged", member);
            AnalyzerAssert.CodeFix<INPC004UseCallerMemberName, UseCallerMemberNameCodeFixProvider>(testCode, fixedCode);
            AnalyzerAssert.FixAll<INPC004UseCallerMemberName, UseCallerMemberNameCodeFixProvider>(testCode, fixedCode);
        }

        [TestCase("this.OnPropertyChanged")]
        [TestCase("OnPropertyChanged")]
        public void ChainedInvoker(string member)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(↓string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
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
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
            testCode = testCode.AssertReplace("this.OnPropertyChanged", member);
            fixedCode = fixedCode.AssertReplace("this.OnPropertyChanged", member);
            AnalyzerAssert.CodeFix<INPC004UseCallerMemberName, UseCallerMemberNameCodeFixProvider>(testCode, fixedCode);
            AnalyzerAssert.FixAll<INPC004UseCallerMemberName, UseCallerMemberNameCodeFixProvider>(testCode, fixedCode);
        }
    }
}