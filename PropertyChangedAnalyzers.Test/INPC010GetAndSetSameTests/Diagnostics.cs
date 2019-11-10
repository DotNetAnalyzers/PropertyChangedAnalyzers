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
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
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
            var c1 = @"
namespace N
{
    public class C1
    {
        public int C1Value;
        public int OtherValue;
    }
}";
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓Value
        {
            get => this.c1.OtherValue;
            set
            {
                if (value == this.c1.C1Value)
                {
                    return;
                }

                this.c1.C1Value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, c1, code);
        }

        [Test]
        public static void DifferentNestedProperties()
        {
            var c1 = @"
namespace N
{
    public class C1
    {
        public int P1 { get; set; }
        public int P2 { get; set; }
    }
}";
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓Value
        {
            get => this.c1.P1;
            set
            {
                if (value == this.c1.P2)
                {
                    return;
                }

                this.c1.P2 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, c1, code);
        }

        [Test]
        public static void WhenSettingNestedFieldRootLevel()
        {
            var c1 = @"
namespace N
{
    public class C1
    {
        public int C1Value;
    }
}";
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c11 = new C1();
        private readonly C1 c12 = new C1();
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓Value
        {
            get => this.c11.C1Value;
            set
            {
                if (value == this.c12.C1Value)
                {
                    return;
                }

                this.c12.C1Value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, c1, code);
        }
    }
}
