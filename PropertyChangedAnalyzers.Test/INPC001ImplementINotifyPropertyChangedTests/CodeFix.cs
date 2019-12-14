namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ClassDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new ImplementINotifyPropertyChangedFix();
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
        private int value;

        public int Value
        {
            get
            {
                return this.value;
            }
            private set
            {
                this.value = value;
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private int value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return this.value;
            }
            private set
            {
                this.value = value;
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
        private int value;

        public int Value
        {
            get => this.value;
            private set => this.value = value;
        }
    }
}";

            var after = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private int value;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get => this.value;
            private set => this.value = value;
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

        [TestCaseSource(typeof(Code), nameof(Code.AutoDetectedStyles))]
        public static void WhenNotNotifyingWithBackingFieldStyleDetection(AutoDetectedStyle style)
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
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(
                Analyzer,
                Fix,
                ExpectedDiagnostic,
                new[] { style.AdditionalSample, style.Apply(before, "p") },
                style.Apply(after, "p"),
                fixTitle: "Implement INotifyPropertyChanged fully qualified.");
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
        public event PropertyChangedEventHandler PropertyChanged;
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

        [TestCase("this.Value = 1;")]
        [TestCase("this.Value++")]
        [TestCase("this.Value--")]
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
            Bar += (_, __) => this.Value = 1;
        }

        public event EventHandler Bar;

        public int Value { get; private set; }
    }
}".AssertReplace("this.Value = 1", assignCode);

            var after = @"
namespace N
{
    using System;

    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        public C()
        {
            Bar += (_, __) => this.Value = 1;
        }

        public event EventHandler Bar;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Value { get; private set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.Value = 1", assignCode);

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
