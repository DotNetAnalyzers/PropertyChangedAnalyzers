namespace PropertyChangedAnalyzers.Test.INPC010GetAndSetSame
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly SetAccessorAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC010GetAndSetSame);

        [Test]
        public static void Message()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int f1 = 1;
        private int f2;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("The property gets and sets a different backing member."), code);
        }

        [Test]
        public static void DifferentFieldsStatementBodies()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int f1 = 1;
        private int f2;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void DifferentFieldsExpressionBodies()
        {
            var code = @"
#pragma warning disable CS0067
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int f1 = 1;
        private int f2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P
        {
            get => this.f1;
            set => this.f2 = value;
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("The property gets and sets a different backing member."), code);
        }

        [Test]
        public static void DifferentFieldsTrySetExpressionBodies()
        {
            var code = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int f1 = 1;
        private int f2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P
        {
            get => this.f1;
            set => this.TrySet(ref this.f2, value);
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void DifferentFieldsTrySetStatementBodies()
        {
            var code = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int f1 = 1;
        private int f2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P
        {
            get { return this.f1; }
            set { this.TrySet(ref this.f2, value); }
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private int f1 = 1;
        private int f2;

        public event PropertyChangedEventHandler? PropertyChanged;

        internal int ↓P
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        public int F1;
        public int F2;
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
        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P
        {
            get => this.c1.F2;
            set
            {
                if (value == this.c1.F1)
                {
                    return;
                }

                this.c1.F1 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

    public class C : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();
        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, c1, code);
        }

        [Test]
        public static void DifferentInstanceFieldsNestedProperties()
        {
            var c1 = @"
namespace N
{
    public class C1
    {
        public int P { get; set; }
    }
}";
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private readonly C1 f1 = new C1();
        private readonly C1 f2 = new C1();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P
        {
            get => this.f1.P;
            set
            {
                if (value == this.f2.P)
                {
                    return;
                }

                this.f2.P = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, c1, code);
        }

        [Test]
        public static void DifferentInstancePropertiesNestedProperties()
        {
            var c1 = @"
namespace N
{
    public class C1
    {
        public int P { get; set; }
    }
}";
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public C1 P1 { get; } = new C1();

        public C1 P2 { get; } = new C1();

        public int ↓P3
        {
            get => this.P1.P;
            set
            {
                if (value == this.P2.P)
                {
                    return;
                }

                this.P2.P = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        public int F;
    }
}";
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private readonly C1 c11 = new C1();
        private readonly C1 c12 = new C1();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P
        {
            get => this.c11.F;
            set
            {
                if (value == this.c12.F)
                {
                    return;
                }

                this.c12.F = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, c1, code);
        }
    }
}
