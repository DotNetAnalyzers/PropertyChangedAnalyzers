namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        private static readonly INPC001ImplementINotifyPropertyChanged Analyzer = new INPC001ImplementINotifyPropertyChanged();

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void CallsOnPropertyChanged(string propertyName)
        {
            var testCode = @"
namespace RoslynSandBox
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
                this.OnPropertyChanged(nameof(Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void CallsRaisePropertyChangedWithEventArgs(string propertyName)
        {
            var testCode = @"
namespace RoslynSandBox
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
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CallsRaisePropertyChangedCallerMemberName()
        {
            var testCode = @"
namespace RoslynSandBox
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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void Invokes(string propertyName)
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar))));
            }
        }
    }
}";
            testCode = testCode.AssertReplace(@"nameof(this.Bar))", propertyName);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void InvokesCached()
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs BarPropertyChangedArgs = new PropertyChangedEventArgs(nameof(Bar));
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, BarPropertyChangedArgs);
            }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}