namespace PropertyChangedAnalyzers.Test.INPC004UseCallerMemberName
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class FixAll
    {
        private static readonly ArgumentAnalyzer Analyzer = new();
        private static readonly UseCallerMemberNameFix Fix = new UseCallerMemberNameFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC004UseCallerMemberName);

        [Test]
        public static void FixAllTest()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p2;
        private int p3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p2 + this.p3;

        public int P2
        {
            get => this.p2;
            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged(↓nameof(this.P2));
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        public int P3
        {
            get => this.p3;
            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged(↓nameof(this.P3));
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p2;
        private int p3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p2 + this.p3;

        public int P2
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

        public int P3
        {
            get => this.p3;
            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyExplicitNameOf()
        {
            var viewModelBaseCode = @"
namespace N.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value, ↓nameof(P2)))
                {
                    this.OnPropertyChanged(nameof(P1));
                }
            }
        }
    }
}";

            var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(P1));
                }
            }
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after);
        }
    }
}
