namespace PropertyChangedAnalyzers.Test.INPC009NotifiesForMissingProperty
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class Argument
        {
            private static readonly ArgumentAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC009NotifiesForMissingProperty);

            [TestCase(@"↓""Missing""")]
            [TestCase(@"nameof(↓p)")]
            [TestCase(@"nameof(this.↓p)")]
            [TestCase(@"nameof(↓PropertyChanged)")]
            [TestCase(@"nameof(this.↓PropertyChanged)")]
            public static void CallsOnPropertyChangedWithExplicitNameOfCaller(string propertyName)
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
                this.OnPropertyChanged(↓""Missing"");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace(@"↓""Missing""", propertyName);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase(@"↓""Missing""")]
            [TestCase(@"nameof(↓p)")]
            [TestCase(@"nameof(this.↓p)")]
            [TestCase(@"nameof(↓PropertyChanged)")]
            [TestCase(@"nameof(this.↓PropertyChanged)")]
            public static void CallsRaisePropertyChangedWithEventArgs(string propertyName)
            {
                var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(↓""Missing""));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace(@"↓""Missing""", propertyName);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase(@"↓""Missing""")]
            [TestCase(@"nameof(↓p)")]
            [TestCase(@"nameof(this.↓p)")]
            [TestCase(@"nameof(↓PropertyChanged)")]
            [TestCase(@"nameof(this.↓PropertyChanged)")]
            public static void Invokes(string propertyName)
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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(↓""Missing""));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace(@"↓""Missing""", propertyName);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public static void InvokesSimple()
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
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(↓""Missing""));
            }
        }
    }
}";
                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [TestCase("↓p")]
            [TestCase("this.↓p")]
            [TestCase("↓PropertyChanged")]
            [TestCase("this.↓PropertyChanged")]
            [TestCase("↓M()")]
            [TestCase("this.↓M()")]
            [TestCase("string.↓Empty")]
            public static void ExpressionInvoker(string expression)
            {
                var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(() => this.↓p);
            }
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int M() => 1;
    }
}".AssertReplace("this.↓p", expression);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }

            [Test]
            public static void PropertyChangedInvokeWithCachedEventArgs()
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs CachedArgs = new PropertyChangedEventArgs(""Missing"");
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
                this.PropertyChanged?.Invoke(this, ↓CachedArgs);
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

            [TestCase("private static readonly PropertyChangedEventArgs CachedArgs = new PropertyChangedEventArgs(\"Missing\")")]
            [TestCase("private static PropertyChangedEventArgs CachedArgs { get; } = new PropertyChangedEventArgs(\"Missing\")")]
            public static void CallsOnPropertyChangedWithCachedEventArgs(string cached)
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs CachedArgs = new PropertyChangedEventArgs(""Missing"");
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.OnPropertyChanged(↓CachedArgs);
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace("private static readonly PropertyChangedEventArgs CachedArgs = new PropertyChangedEventArgs(\"Missing\")", cached);

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
