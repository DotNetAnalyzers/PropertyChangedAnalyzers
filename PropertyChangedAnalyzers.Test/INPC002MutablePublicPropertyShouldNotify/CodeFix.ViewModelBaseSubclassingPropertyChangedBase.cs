namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class ViewModelBaseInheritsPropertyChangedBase
        {
            private const string ViewModelBaseCode = @"
namespace N.Core
{
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : Caliburn.Micro.PropertyChangedBase
    {
        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            return base.Set(ref field, value, propertyName);
        }
    }
}";

            private const string ViewModelBaseUnderscore = @"
namespace N
{
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : Caliburn.Micro.PropertyChangedBase
    {
        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            return base.Set(ref field, value, propertyName);
        }

        int M() => M();
    }
}";

            private static readonly Settings Settings = LibrarySettings.CaliburnMicro;

            [Test]
            public static void AutoPropertyToNotifyWhenValueChanges()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

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
                this.NotifyOfPropertyChange();
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "Notify when value changes.", settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "Notify when value changes.", settings: Settings);
            }

            [Test]
            public static void AutoPropertyToTrySet()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P { get => this.p; set => this.TrySet(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
            }

            [Test]
            public static void AutoPropertyInitializedToSet()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public int ↓P { get; set; } = 1;
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p = 1;

        public int P { get => this.p; set => this.TrySet(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
            }

            [Test]
            public static void AutoPropertyVirtualToSet()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public virtual int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public virtual int P { get => this.p; set => this.TrySet(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
            }

            [Test]
            public static void AutoPropertyPrivateSetToSet()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public int ↓P { get; private set; }

        public void Mutate()
        {
            this.P++;
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P { get => this.p; private set => this.TrySet(ref this.p, value); }

        public void Mutate()
        {
            this.P++;
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
            }

            [Test]
            public static void AutoPropertyToTrySetUnderscoreNames()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public C(int p)
        {
            P = p;
        }

        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int _p;

        public C(int p)
        {
            P = p;
        }

        public int P { get => _p; set => TrySet(ref _p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
            }

            [Test]
            public static void WithBackingFieldToSetStatementBody()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int ↓P
        {
            get { return this.p; }
            set { this.p = value; }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set { this.TrySet(ref this.p, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
            }

            [Test]
            public static void WithBackingFieldToSetExpressionBody()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int ↓P
        {
            get => this.p;
            set => this.p = value;
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesStatementBody()
            {
                var before = @"
namespace N
{
    public class C : N.ViewModelBase
    {
        private int _p;

        public int ↓P
        {
            get { return _p; }
            set { _p = value; }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : N.ViewModelBase
    {
        private int _p;

        public int P
        {
            get { return _p; }
            set { TrySet(ref _p, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesExpressionBody()
            {
                var before = @"
namespace N
{
    public class C : ViewModelBase
    {
        private int _p;

        public int ↓P
        {
            get => _p;
            set => _p = value;
        }
    }
}";

                var after = @"
namespace N
{
    public class C : ViewModelBase
    {
        private int _p;

        public int P
        {
            get => _p;
            set => TrySet(ref _p, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)", settings: Settings);
            }

            [Test]
            public static void AutoPropertyWhenRecursionInTrySet()
            {
                var viewModelBaseCode = @"
namespace N.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            return this.TrySet(ref field, value, propertyName);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

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
                this.OnPropertyChanged();
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after, settings: Settings);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after, settings: Settings);
            }
        }
    }
}
