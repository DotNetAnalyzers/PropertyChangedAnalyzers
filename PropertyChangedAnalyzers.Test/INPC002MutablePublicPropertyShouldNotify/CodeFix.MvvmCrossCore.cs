namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify;

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using PropertyChangedAnalyzers.Test.Helpers;

public static partial class CodeFix
{
    public static class MvvmCrossCore
    {
        private static readonly Settings Settings = LibrarySettings.MvvmCross;

        [Test]
        public static void AutoPropertyToNotifyWhenValueChanges()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int ↓P { get; set; }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
                this.RaisePropertyChanged();
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.", settings: Settings);
        }

        [Test]
        public static void AutoPropertyToTrySet()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int ↓P { get; set; }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int p;

        public int P { get => this.p; set => this.SetProperty(ref this.p, value); }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
        }

        [Test]
        public static void InternalClassInternalPropertyAutoPropertyToTrySet()
        {
            var before = @"
namespace N
{
    internal class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        internal int ↓P { get; set; }
    }
}";

            var after = @"
namespace N
{
    internal class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int p;

        internal int P { get => this.p; set => this.SetProperty(ref this.p, value); }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
        }

        [Test]
        public static void AutoPropertyInitializedToSet()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int ↓P { get; set; } = 1;
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int p = 1;

        public int P { get => this.p; set => this.SetProperty(ref this.p, value); }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
        }

        [Test]
        public static void AutoPropertyVirtualToSet()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public virtual int ↓P { get; set; }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int p;

        public virtual int P { get => this.p; set => this.SetProperty(ref this.p, value); }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
        }

        [Test]
        public static void AutoPropertyPrivateSetToSet()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public int ↓P { get; private set; }

        public void Mutate()
        {
            this.P++;
        }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int p;

        public int P { get => this.p; private set => this.SetProperty(ref this.p, value); }

        public void Mutate()
        {
            this.P++;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
        }

        [Test]
        public static void AutoPropertyToTrySetUnderscoreNames()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        public C(int p)
        {
            P = p;
        }

        public int ↓P { get; set; }
    }
}";

            var after = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int _p;

        public C(int p)
        {
            P = p;
        }

        public int P { get => _p; set => SetProperty(ref _p, value); }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
        }

        [Test]
        public static void WithBackingFieldToSetStatementBody()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set { this.SetProperty(ref this.p, value); }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
        }

        [Test]
        public static void WithBackingFieldToSetExpressionBody()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int p;

        public int P
        {
            get => this.p;
            set => this.SetProperty(ref this.p, value);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
        }

        [Test]
        public static void WithBackingFieldToSetUnderscoreNames()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int _p;

        public int P
        {
            get { return _p; }
            set { SetProperty(ref _p, value); }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
        }

        [Test]
        public static void WithBackingFieldToSetUnderscoreNamesExpressionBody()
        {
            var before = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
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
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int _p;

        public int P
        {
            get => _p;
            set => SetProperty(ref _p, value);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "SetProperty(ref storage, value)", settings: Settings);
        }
    }
}
