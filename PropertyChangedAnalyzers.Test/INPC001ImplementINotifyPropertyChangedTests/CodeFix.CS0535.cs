namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class CS0535
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0535");

            [Test]
            public static void WhenInterfaceAndUsingSealedAddUsings()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public sealed class C : ↓INotifyPropertyChanged
    {
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged and add usings.");
            }

            [Test]
            public static void WhenInterfaceAndUsingSealedFullyQualified()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public sealed class C : ↓INotifyPropertyChanged
    {
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;

    public sealed class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            }

            [Test]
            public static void WhenInterfaceOnlyWithUsingAddUsing()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : ↓INotifyPropertyChanged
    {
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged and add usings.");
            }

            [Test]
            public static void WhenInterfaceOnlyWithUsingFullyQualified()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : ↓INotifyPropertyChanged
    {
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            }

            [TestCaseSource(typeof(Code), nameof(Code.AutoDetectedStyles))]
            public static void WhenInterfaceOnlyWithUsingAddUsingsStyleDetection(AutoDetectedStyle style)
            {
                var before = @"
#pragma warning disable 169
namespace N
{
    using System.ComponentModel;

    public class C : ↓INotifyPropertyChanged
    {
        private int p;
    }
}";

                var after = @"
#pragma warning disable 169
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(
                    Fix,
                    ExpectedDiagnostic,
                    new[] { style.AdditionalSample, style.Apply(before, "p") },
                    style.Apply(after, "p"),
                    fixTitle: "Implement INotifyPropertyChanged and add usings.");
            }

            [Test]
            public static void WhenInterfaceOnlyWithUsingUnderscoreFullyQualified()
            {
                var before = @"
#pragma warning disable 169
namespace N
{
    using System.ComponentModel;

    public class C : ↓INotifyPropertyChanged
    {
        private int _value;
    }
}";

                var after = @"
#pragma warning disable 169
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            }

            [Test]
            public static void WhenInterfaceOnlyAndUsingsAddUsing()
            {
                var before = @"
#pragma warning disable 8019
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : ↓INotifyPropertyChanged
    {
    }
}";

                var after = @"
#pragma warning disable 8019
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged and add usings.");
            }

            [Test]
            public static void WhenInterfaceOnlyAndUsingsFullyQualified()
            {
                var before = @"
#pragma warning disable 8019
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : ↓INotifyPropertyChanged
    {
    }
}";

                var after = @"
#pragma warning disable 8019
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            }
        }
    }
}
