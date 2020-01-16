namespace PropertyChangedAnalyzers.Test.INPC019GetBackingFieldTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new ReplaceExpressionFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC019GetBackingField);

        [Test]
        public static void SimpleExpressionBody()
        {
            var before = @"
namespace N
{
    public sealed class C
    {
        private string p;

        public string P
        {
            get => ↓""abc"";
            set => this.p = value;
        }
    }
}";
            var after = @"
namespace N
{
    public sealed class C
    {
        private string p;

        public string P
        {
            get => this.p;
            set => this.p = value;
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ExpressionBody()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get => ↓""abc"";

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get => this.p2;

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenFieldIsKeyword()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string @default;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.@default}"";

        public string Default
        {
            get => ↓""abc"";

            set
            {
                if (value == this.@default)
                {
                    return;
                }

                this.@default = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string @default;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.@default}"";

        public string Default
        {
            get => this.@default;

            set
            {
                if (value == this.@default)
                {
                    return;
                }

                this.@default = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void TrySetExpressionBody()
        {
            var before = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p2;
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get => ↓""abc"";

            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }

        public int P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }
}";

            var after = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p2;
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get => this.p2;

            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }

        public int P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void StatementBody()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get
            {
                return ↓""abc"";
            }

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get
            {
                return this.p2;
            }

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
