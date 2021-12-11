namespace PropertyChangedAnalyzers.Test.INPC012DoNotUseExpression
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class ViewModelBase
        {
            private const string ViewModelBaseCode = @"
namespace N.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            [Test]
            public static void SetAffectsCalculatedPropertyExpression()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string? p2;

        public string P1 => $""Hello {this.p2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(↓() => this.P1);
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
        private string? p2;

        public string P1 => $""Hello {this.p2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpressionInternalClassInternalProperty()
            {
                var before = @"
namespace N.Client
{
    internal class C : N.Core.ViewModelBase
    {
        private string? p2;

        internal string P1 => $""Hello {this.p2}"";

        internal string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(↓() => this.P1);
                }
            }
        }
    }
}";

                var after = @"
namespace N.Client
{
    internal class C : N.Core.ViewModelBase
    {
        private string? p2;

        internal string P1 => $""Hello {this.p2}"";

        internal string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after);
            }
        }
    }
}
