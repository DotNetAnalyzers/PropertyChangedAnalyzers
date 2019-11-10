namespace PropertyChangedAnalyzers.Test.INPC012DoNotUseExpressionTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ArgumentAnalyzer();
        private static readonly CodeFixProvider Fix = new RemoveExpressionFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC012DoNotUseExpression);

        [Test]
        public static void ExpressionInvokerToCallerMemberName()
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
                this.OnPropertyChanged(↓() => this.Value);
            }
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

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ExpressionInvokerCalculatedCallerMemberName()
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Square => this.value * this.value;

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
                this.OnPropertyChanged(↓() => this.Square);
            }
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

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Square => this.value * this.value;

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
                this.OnPropertyChanged(nameof(this.Square));
            }
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ExpressionInvokerCalculatedCallerMemberNameUnderscore()
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Square => _value * _value;

        public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value == _value)
                {
                    return;
                }

                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(↓() => Square);
            }
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Square => _value * _value;

        public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value == _value)
                {
                    return;
                }

                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Square));
            }
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ExpressionInvokerToCallerMemberNameInternalClassInternalProperty()
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
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
                this.OnPropertyChanged(↓() => this.Value);
            }
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

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ExpressionInvokerToCallerMemberNameCalculatedProperty()
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting => $""Hello { this.Name}"";

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(↓() => this.Greeting);
            }
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

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting => $""Hello { this.Name}"";

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Greeting));
            }
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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ExpressionInvokerToNameOf()
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    public class C : INotifyPropertyChanged
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
                this.OnPropertyChanged(↓() => this.Value);
            }
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    public class C : INotifyPropertyChanged
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
                this.OnPropertyChanged(nameof(this.Value));
            }
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
