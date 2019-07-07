namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class ValidCode
    {
        public static class CaliburnMicro
        {
            [OneTimeSetUp]
            public static void OneTimeSetUp()
            {
                RoslynAssert.AddTransitiveMetadataReferences(typeof(Caliburn.Micro.PropertyChangedBase).Assembly);
            }

            [OneTimeTearDown]
            public static void TearDown()
            {
                RoslynAssert.ResetMetadataReferences();
            }

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var code = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyEmptyIf()
            {
                var code = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.Greeting));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void SetAffectsSecondCalculatedProperty()
            {
                var code = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.Greeting1));
                    this.NotifyOfPropertyChange(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var code = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.NotifyOfPropertyChange(nameof(this.Greeting1));
                    this.NotifyOfPropertyChange(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void NotifyOfPropertyChangeAffectsCalculatedProperty()
            {
                var code = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private string firstName;
        private string lastName;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string FirstName
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

        public string LastName
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
                RoslynAssert.Valid(Analyzer, code);
            }

            [Test]
            public static void IfNotSetReturnCalculatedProperty()
            {
                var code = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (!Set(ref this.name, value))
                {
                    return;
                }

                this.NotifyOfPropertyChange(nameof(this.Greeting));
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code);
            }
        }
    }
}
