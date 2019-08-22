namespace PropertyChangedAnalyzers.Test.INPC010GetAndSetSameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC010GetAndSetSame);

        [Test]
        public static void DifferentFieldsAssign()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int f1;
        private int f2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get
            {
                return this.f1;
            }

            set
            {
                if (value == this.f2)
                {
                    return;
                }

                this.f2 = value;
                this.OnPropertyChanged(nameof(P));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("The property sets a different field than it returns."), code);
        }

        [Test]
        public static void DifferentFieldsTrySet()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int f1;
        private int f2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get => this.f1;
            set => this.TrySet(ref this.f2, value);
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void DifferentFieldsInternal()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class ViewModel : INotifyPropertyChanged
    {
        private int otherValue;
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int ↓Value
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void DifferentNestedFields()
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
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓Value
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
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, barCode, code);
        }

        [Test]
        public static void DifferentNestedProperties()
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
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓Value
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
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, barCode, code);
        }

        [Test]
        public static void WhenSettingNestedFieldRootLevel()
        {
            var barCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int BarValue;
    }
}";
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar1 = new Bar();
        private readonly Bar bar2 = new Bar();
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓Value
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
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, barCode, code);
        }
    }
}
