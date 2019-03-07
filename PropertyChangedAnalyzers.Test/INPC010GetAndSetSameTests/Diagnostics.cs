namespace PropertyChangedAnalyzers.Test.INPC010GetAndSetSameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(INPC010GetAndSetSame.Descriptor);

        [Test]
        public void DifferentFields()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int otherValue;
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get
            {
                return this.otherValue;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged(nameof(Value));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void DifferentFieldsInternal()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ViewModel : INotifyPropertyChanged
    {
        private int otherValue;
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        ↓internal int Value
        {
            get
            {
                return this.otherValue;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged(nameof(Value));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void DifferentNestedFields()
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int BarValue;
        public int OtherValue;
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get => this.bar.OtherValue;
            set
            {
                if (value == this.bar.BarValue)
                {
                    return;
                }

                this.bar.BarValue = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, barCode, testCode);
        }

        [Test]
        public void DifferentNestedProperties()
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int P1 { get; set; }
        public int P2 { get; set; }
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get => this.bar.P1;
            set
            {
                if (value == this.bar.P2)
                {
                    return;
                }

                this.bar.P2 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, barCode, testCode);
        }

        [Test]
        public void WhenSettingNestedFieldRootLevel()
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int BarValue;
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar1 = new Bar();
        private readonly Bar bar2 = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get => this.bar1.BarValue;
            set
            {
                if (value == this.bar2.BarValue)
                {
                    return;
                }

                this.bar2.BarValue = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, barCode, testCode);
        }
    }
}
