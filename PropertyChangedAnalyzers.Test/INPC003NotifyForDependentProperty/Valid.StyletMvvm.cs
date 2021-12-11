namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentProperty
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class Valid
    {
        public static class StyletMvvm
        {
            private static readonly Settings Settings = LibrarySettings.Stylet;

            [Test]
            public static void SetProperty()
            {
                var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string? p;

        public string? P
        {
            get { return this.p; }
            set { this.SetAndNotify(ref this.p, value); }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, settings: Settings);
            }

            [Test]
            public static void SetPropertyExpressionBodies()
            {
                var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string? p;

        public string? P
        {
            get => this.p;
            set => this.SetAndNotify(ref this.p, value);
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, settings: Settings);
            }

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetAndNotify(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(P1));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, settings: Settings);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyStringEmpty()
            {
                var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get => this.p2;
            set => this.SetAndNotify(ref this.p2, value, string.Empty);
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, settings: Settings);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpression()
            {
                var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string? p2;

        public string P1 => $""Hello{this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetAndNotify(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(() => this.P1);
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, settings: Settings);
            }

            [Test]
            public static void WhenOverriddenSetAndNotify()
            {
                var viewModelBase = @"
namespace N
{
    public abstract class ViewModelBase : Stylet.PropertyChangedBase
    {
        protected override bool SetAndNotify<T>(ref T oldValue, T value,[System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            return base.SetAndNotify(ref oldValue, value, propertyName);
        }
    }
}";

                var code = @"
namespace N
{
    public class C : ViewModelBase
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set { this.SetAndNotify(ref this.p, value); }
        }
    }
}";

                RoslynAssert.Valid(Analyzer, new[] { viewModelBase, code }, settings: Settings);
            }
        }
    }
}
