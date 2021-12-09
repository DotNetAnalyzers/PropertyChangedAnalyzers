namespace PropertyChangedAnalyzers.Test.INPC012DoNotUseExpression
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class CodeFix
    {
        public static class CaliburnMicro
        {
            private static readonly Settings Settings = LibrarySettings.CaliburnMicro;

            [Test]
            public static void SetAffectsCalculatedPropertyExpression()
            {
                var before = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(↓() => this.P1);
                }
            }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyExpressionInternalClassInternalProperty()
            {
                var before = @"
namespace N
{
    internal class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        internal string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(↓() => this.P1);
                }
            }
        }
    }
}";

                var after = @"
namespace N
{
    internal class C : Caliburn.Micro.PropertyChangedBase
    {
        private string p2;

        public string P1 => $""Hello {this.p2}"";

        internal string P2
        {
            get { return this.p2; }
            set
            {
                if (this.Set(ref this.p2, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings);
            }
        }
    }
}
