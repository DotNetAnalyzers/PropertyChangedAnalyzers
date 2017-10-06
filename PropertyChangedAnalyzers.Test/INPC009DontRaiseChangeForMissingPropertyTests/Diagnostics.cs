namespace PropertyChangedAnalyzers.Test.INPC009DontRaiseChangeForMissingPropertyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        [TestCase(@"""Missing""")]
        [TestCase(@"nameof(PropertyChanged)")]
        [TestCase(@"nameof(this.PropertyChanged)")]
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

            testCode = testCode.AssertReplace(@"nameof(Value)", propertyName);
            AnalyzerAssert.Diagnostics<INPC009DontRaiseChangeForMissingProperty>(testCode);
        }

        [TestCase(@"""Missing""")]
        [TestCase(@"nameof(PropertyChanged)")]
        [TestCase(@"nameof(this.PropertyChanged)")]
        public void CallsRaisePropertyChangedWithEventArgs(string propertyName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(↓nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            AnalyzerAssert.Diagnostics<INPC009DontRaiseChangeForMissingProperty>(testCode);
        }

        [TestCase(@"""Missing""")]
        [TestCase(@"nameof(PropertyChanged)")]
        [TestCase(@"nameof(this.PropertyChanged)")]
        public void Invokes(string propertyName)
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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(↓nameof(Value)));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            testCode = testCode.AssertReplace(@"nameof(Value)", propertyName);
            AnalyzerAssert.Diagnostics<INPC009DontRaiseChangeForMissingProperty>(testCode);
        }

        [Test]
        public void InvokesSimple()
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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(↓""MIssing""));
            }
        }
    }
}";
            AnalyzerAssert.Diagnostics<INPC009DontRaiseChangeForMissingProperty>(testCode);
        }

        [Test]
        public void InvokesCached()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs CachedArgs = new PropertyChangedEventArgs(""Missing"");
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
                this.PropertyChanged?.Invoke(this, ↓CachedArgs);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.Diagnostics<INPC009DontRaiseChangeForMissingProperty>(testCode);
        }

        [Test]
        public void CallsOnPropertyChangedWithCachedEventArgs()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs CachedArgs = new PropertyChangedEventArgs(""Missing"");
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged(↓CachedArgs);
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
            AnalyzerAssert.Diagnostics<INPC009DontRaiseChangeForMissingProperty>(testCode);
        }
    }
}