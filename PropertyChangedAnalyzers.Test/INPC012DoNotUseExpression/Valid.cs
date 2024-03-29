namespace PropertyChangedAnalyzers.Test.INPC012DoNotUseExpression;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly ArgumentAnalyzer Analyzer = new();

    [Test]
    public static void NoBaseClass()
    {
        var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Square => this.P * this.P;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Square));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void CallerMemberName()
    {
        var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void NotInvoker()
    {
        var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Square => this.P * this.P;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(Square));
            }
        }

        public PropertyInfo GetProperty()
        {
            return GetProperty(() => this.P);
        }

        protected virtual PropertyInfo GetProperty<T>(Expression<Func<T>> expression)
        {
            return (PropertyInfo)((MemberExpression)expression.Body).Member;
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }
}
