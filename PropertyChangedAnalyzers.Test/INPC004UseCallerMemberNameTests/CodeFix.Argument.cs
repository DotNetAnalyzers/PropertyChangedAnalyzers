namespace PropertyChangedAnalyzers.Test.INPC004UseCallerMemberNameTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    internal partial class CodeFix
    {
        public class Argument
        {
            private static readonly ArgumentAnalyzer Analyzer = new ArgumentAnalyzer();
            private static readonly UseCallerMemberNameCodeFixProvider CodeFix = new UseCallerMemberNameCodeFixProvider();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("INPC004");

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.MetadataReferences.Add(SpecialMetadataReferences.Stylet);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetAll();
            }

            [TestCase(@"""Value""")]
            [TestCase(@"nameof(Value)")]
            [TestCase(@"nameof(this.Value)")]
            public void CallsOnPropertyChangedWithExplicitNameOfCallerWhenParameterIsCallerMemberName(string propertyName)
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
                AnalyzerAssert.CodeFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
            }

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

        protected virtual void OnPropertyChanged(string propertyName)
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
                AnalyzerAssert.CodeFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void InternalClassInternalProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int Value
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

    internal class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int Value
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
                AnalyzerAssert.CodeFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void SecondArgument()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int Value
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
                this.OnPropertyChanged(1, ↓nameof(Value));
            }
        }

        protected virtual void OnPropertyChanged(int i, [CallerMemberName] string propertyName = null)
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

    internal class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int Value
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
                this.OnPropertyChanged(1);
            }
        }

        protected virtual void OnPropertyChanged(int i, [CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void StyletSetAndNotify()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            set => this.SetAndNotify(ref this.bar, value, ↓nameof(this.Bar));
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            set => this.SetAndNotify(ref this.bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void FixAll()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value1;
        private int value2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Sum => this.value1 + this.value2;

        public int Value1
        {
            get
            {
                return this.value1;
            }

            set
            {
                if (value == this.value1)
                {
                    return;
                }

                this.value1 = value;
                this.OnPropertyChanged(↓nameof(Value1));
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

        public int Value2
        {
            get
            {
                return this.value2;
            }

            set
            {
                if (value == this.value2)
                {
                    return;
                }

                this.value2 = value;
                this.OnPropertyChanged(↓nameof(Value2));
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
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
        private int value1;
        private int value2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Sum => this.value1 + this.value2;

        public int Value1
        {
            get
            {
                return this.value1;
            }

            set
            {
                if (value == this.value1)
                {
                    return;
                }

                this.value1 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

        public int Value2
        {
            get
            {
                return this.value2;
            }

            set
            {
                if (value == this.value2)
                {
                    return;
                }

                this.value2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.FixAll(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
            }
        }
    }
}
