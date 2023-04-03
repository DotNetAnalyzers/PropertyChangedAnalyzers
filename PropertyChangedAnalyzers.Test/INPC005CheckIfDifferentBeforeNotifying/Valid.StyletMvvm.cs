namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifying;

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using PropertyChangedAnalyzers.Test.Helpers;

public static partial class Valid
{
    public static class StyletMvvm
    {
        private static readonly Settings Settings = LibrarySettings.Stylet;

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
                    this.NotifyOfPropertyChange(nameof(this.P1));
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code, settings: Settings);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyEmptyIf()
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
                    this.NotifyOfPropertyChange(nameof(this.P1));
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code, settings: Settings);
        }

        [Test]
        public static void SetAffectsSecondCalculatedProperty()
        {
            var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string? p3;

        public string P1 => $""Hello {this.P3}"";

        public string P2 => $""Hej {this.P3}"";

        public string? P3
        {
            get { return this.p3; }
            set
            {
                if (this.SetAndNotify(ref this.p3, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P1));
                    this.NotifyOfPropertyChange(nameof(this.P2));
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code, settings: Settings);
        }

        [Test]
        public static void SetAffectsSecondCalculatedPropertyMissingBraces()
        {
            var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string? p3;

        public string P1 => $""Hello {this.P3}"";

        public string P2 => $""Hej {this.P3}"";

        public string? P3
        {
            get { return this.p3; }
            set
            {
                if (this.SetAndNotify(ref this.p3, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.P1));
                    this.NotifyOfPropertyChange(nameof(this.P2));
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code, settings: Settings);
        }

        [Test]
        public static void NotifyOfPropertyChangeAffectsCalculatedProperty()
        {
            var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private string? firstName;
        private string? lastName;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.NotifyOfPropertyChange();
                this.NotifyOfPropertyChange(nameof(this.FullName));
            }
        }

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.NotifyOfPropertyChange();
                this.NotifyOfPropertyChange(nameof(this.FullName));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code, settings: Settings);
        }

        [Test]
        public static void IfNotSetReturnCalculatedProperty()
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
                if (!SetAndNotify(ref this.p2, value))
                {
                    return;
                }

                this.NotifyOfPropertyChange(nameof(this.P1));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code, settings: Settings);
        }
    }
}
