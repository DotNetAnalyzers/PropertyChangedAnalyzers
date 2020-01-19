namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetAccessorAnalyzer();
        private static readonly CodeFixProvider Fix = new MakePropertyNotifyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC002MutablePublicPropertyShouldNotify);

        [Test]
        public static void CallsOnPropertyChangedCopyLocalNullCheckInvoke()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AutoPropertyPublic()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AutoPropertyPublicPrivateNeverAssignedIssue158()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; private set; }

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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.p;
            private set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AutoPropertyInternalClass()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AutoPropertyInternal()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal int ↓P { get; set; }

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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int P
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
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
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public C()
        {
            E += (_, __) => this.P = 1;
        }

        public event EventHandler E;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; private set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.P = 1", assignCode);

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public C()
        {
            E += (_, __) => this.P = 1;
        }

        public event EventHandler E;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.p;
            private set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("this.P = 1", assignCode);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenBackingFieldNotifyWhenValueChanged()
        {
            var before = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private int p;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get => this.p;
            set
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

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.");
        }

        [Test]
        public static void WhenBackingFieldNotifyCallerMemberName()
        {
            var before = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private int p;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get => this.p;
            set => this.p = value;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
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
            set
            {
                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
        }

        [Test]
        public static void WhenBackingFieldExpressionBodyAccessors()
        {
            var before = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private int p;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get => this.p;
            set => this.p = value;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
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

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify when value changes.");
        }

        [Test]
        public static void WhenBackingFieldNotify()
        {
            var before = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private int p;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get => this.p;
            set
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
            set
            {
                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
        }

        [Test]
        public static void WhenBackingFieldExpressionBodyAccessorsNotify()
        {
            var before = @"
namespace N
{
    public class C : System.ComponentModel.INotifyPropertyChanged
    {
        private int p;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get => this.p;
            set => this.p = value;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
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
            set
            {
                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
        }

        [Test]
        public static void WhenSettingNestedField()
        {
            var c1 = @"
namespace N
{
    public class C1
    {
        public int P;
    }
}";
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get => this.c1.P;
            set => this.c1.P = value;
        }

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

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.c1.P;
            set
            {
                if (value == this.c1.P)
                {
                    return;
                }

                this.c1.P = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after, fixTitle: "Notify when value changes.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after, fixTitle: "Notify when value changes.");
        }

        [Test]
        public static void WhenSettingNestedFieldNotify()
        {
            var c1 = @"
namespace N
{
    public class C1
    {
        public int P;
    }
}";
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get => this.c1.P;
            set => this.c1.P = value;
        }

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

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();
        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.c1.P;
            set
            {
                this.c1.P = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after, fixTitle: "Notify.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after, fixTitle: "Notify.");
        }

        [TestCase("this.P = p;")]
        [TestCase("this.P--;")]
        [TestCase("this.P+= p;")]
        public static void PrivateSetMutatedOutsideCtor(string mutation)
        {
            var viewModelBase = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
        {
            if (!RuntimeHelpers.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var before = @"
namespace N
{
    public sealed class C : ViewModelBase
    {
        public int ↓P { get; private set; }

        public void M(int p)
        {
            this.P = p;
        }
    }
}".AssertReplace("this.P = p;", mutation);

            var after = @"
namespace N
{
    public sealed class C : ViewModelBase
    {
        private int p;

        public int P { get => this.p; private set => Set(ref this.p, value); }

        public void M(int p)
        {
            this.P = p;
        }
    }
}".AssertReplace("this.P = p;", mutation);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBase, before }, after, fixTitle: "Set(ref field, value)");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBase, before }, after, fixTitle: "Set(ref field, value)");
        }

        [Test]
        public static void NullableUseSet()
        {
            var viewModelBase = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
        {
            if (!RuntimeHelpers.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var before = @"
namespace N
{
    public sealed class C : ViewModelBase
    {
        public string? ↓P { get; set; }
    }
}";
            var after = @"
namespace N
{
    public sealed class C : ViewModelBase
    {
        private string? p;

        public string? P { get => this.p; set => Set(ref this.p, value); }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBase, before }, after, fixTitle: "Set(ref field, value)");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBase, before }, after, fixTitle: "Set(ref field, value)");
        }

        [Test]
        public static void NullableUseOnPropertyChanged()
        {
            var viewModelBase = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
        {
            if (!RuntimeHelpers.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var before = @"
namespace N
{
    public sealed class C : ViewModelBase
    {
        public string? ↓P { get; set; }
    }
}";
            var after = @"
namespace N
{
    public sealed class C : ViewModelBase
    {
        private string? p;

        public string? P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                OnPropertyChanged();
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBase, before }, after, fixTitle: "Notify when value changes.");
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBase, before }, after, fixTitle: "Notify when value changes.");
        }
    }
}
