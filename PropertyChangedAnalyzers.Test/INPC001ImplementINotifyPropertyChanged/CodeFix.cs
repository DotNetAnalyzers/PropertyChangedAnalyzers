namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChanged
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        private static readonly ClassDeclarationAnalyzer Analyzer = new();
        private static readonly ImplementINotifyPropertyChangedFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC001ImplementINotifyPropertyChanged);

        [Test]
        public static void Message()
        {
            var before = @"
namespace N
{
    public class ↓C
    {
        public int P1 { get; set; }

        public int P2 { get; set; }
    }
}";

            var after = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int P1 { get; set; }

        public int P2 { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var expectedDiagnostic = ExpectedDiagnostic.WithMessage("The class C should notify for:\r\nP1\r\nP2");
            RoslynAssert.CodeFix(Analyzer, Fix, expectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenPublicClassPublicAutoProperty()
        {
            var before = @"
namespace N
{
    public class ↓C
    {
        public int P { get; set; }
    }
}";

            var after = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int P { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenInternalClassInternalAutoProperty()
        {
            var before = @"
namespace N
{
    internal class ↓C
    {
        internal int P { get; set; }
    }
}";

            var after = @"
namespace N
{
    internal class C : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        internal int P { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenNotNotifyingWithBackingField()
        {
            var before = @"
namespace N
{
    public class ↓C
    {
        private int p;

        public int P
        {
            get
            {
                return this.p;
            }
            private set
            {
                this.p = value;
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private int p;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }
            private set
            {
                this.p = value;
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenNotNotifyingWithBackingFieldExpressionBodies()
        {
            var before = @"
namespace N
{
    public class ↓C
    {
        private int p;

        public int P
        {
            get => this.p;
            private set => this.p = value;
        }
    }
}";

            var after = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private int p;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.p;
            private set => this.p = value;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenNotNotifyingWithBackingFieldUnderscoreNames()
        {
            var before = @"
namespace N
{
    public class ↓C
    {
        private int _p;

        public int P
        {
            get
            {
                return _p;
            }
            private set
            {
                _p = value;
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private int _p;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return _p;
            }
            private set
            {
                _p = value;
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, before }, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenEventOnlyNullableDisabled()
        {
            var before = @"
#nullable disable
namespace N
{
    using System.ComponentModel;

    public class ↓C
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}";

            var after = @"
#nullable disable
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenEventOnly()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class ↓C
    {
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenEventAndInvokerOnly()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ↓C
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [TestCase("this.P = 1;")]
        [TestCase("this.P++")]
        [TestCase("this.P--")]
        public static void WhenPrivateSetAssignedInLambdaInCtor(string assignCode)
        {
            var before = @"
namespace N
{
    using System;

    public class ↓C
    {
        public C()
        {
            E += (_, __) => this.P = 1;
        }

        public event EventHandler E;

        public int P { get; private set; }
    }
}".AssertReplace("this.P = 1", assignCode);

            var after = @"
namespace N
{
    using System;

    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        public C()
        {
            E += (_, __) => this.P = 1;
        }

        public event EventHandler E;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int P { get; private set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.P = 1", assignCode);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }

        [Test]
        public static void WhenInNullableContext()
        {
            var before = @"
#nullable enable

namespace N
{
    public class ↓C
    {
        public int P { get; set; }
    }
}";

            var after = @"
#nullable enable

namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public int P { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Implement INotifyPropertyChanged fully qualified.");
        }
    }
}
