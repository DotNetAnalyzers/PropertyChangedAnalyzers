namespace PropertyChangedAnalyzers.Test.INPC007MissingInvoker
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly EventAnalyzer Analyzer = new();
        private static readonly AddOnPropertyChangedFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC007MissingInvoker);

        [Test]
        public static void EventOnlyAddInvoker()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler? PropertyChanged;
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Add OnPropertyChanged()");
        }

        [Test]
        public static void EventOnlyMakeSealed()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler? PropertyChanged;
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;

    public sealed class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Seal class.");
        }

        [Test]
        public static void EventOnlyWithUsing()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler? PropertyChanged;
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Add OnPropertyChanged()");
        }

        [Test]
        public static void EventOnlySealed()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public sealed class C : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler? PropertyChanged;

        public int P { get; set; }
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;

    public sealed class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int P { get; set; }

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void EventOnlyStatic()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public static class C
    {
        ↓public static event PropertyChangedEventHandler? PropertyChanged;
    }
}";

            var after = @"
namespace N
{
    using System.ComponentModel;

    public static class C
    {
        public static event PropertyChangedEventHandler? PropertyChanged;

        private static void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void OverridingEvent()
        {
            var viewModelBaseCode = @"
namespace N.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var before = @"
namespace N.Client
{
    using System.ComponentModel;

    public class C : N.Core.ViewModelBase
    {
        ↓public override event PropertyChangedEventHandler? PropertyChanged;
    }
}";

            var after = @"
namespace N.Client
{
    using System.ComponentModel;

    public class C : N.Core.ViewModelBase
    {
        public override event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after, fixTitle: "Add OnPropertyChanged()");
        }

        [Test]
        public static void WithNoMutablePropertiesAddInvoker()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public C(int p1)
        {
            this.P1 = p1;
        }

        ↓public event PropertyChangedEventHandler? PropertyChanged;

        public int P1 { get; }

        public int P2 => this.P1 * this.P1;
    }
}";
            var after = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public C(int p1)
        {
            this.P1 = p1;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1 { get; }

        public int P2 => this.P1 * this.P1;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Add OnPropertyChanged()");
        }

        [Test]
        public static void WithNoMutablePropertiesSeal()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public C(int p1)
        {
            this.P1 = p1;
        }

        ↓public event PropertyChangedEventHandler? PropertyChanged;

        public int P1 { get; }

        public int P2 => this.P1 * this.P1;
    }
}";
            var after = @"
namespace N
{
    using System.ComponentModel;

    public sealed class C : INotifyPropertyChanged
    {
        public C(int p1)
        {
            this.P1 = p1;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P1 { get; }

        public int P2 => this.P1 * this.P1;
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Seal class.");
        }

        [Test]
        public static void UsesCorrectStyleIssue107()
        {
            var c1 = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C1 : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var before = @"
namespace N
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class C2 : INotifyPropertyChanged
    {
        ↓public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<C1> Items { get; } = new ObservableCollection<C1>
        {
            new C1 { P = 2 },
        };
    }
}";
            var after = @"
namespace N
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class C2 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<C1> Items { get; } = new ObservableCollection<C1>
        {
            new C1 { P = 2 },
        };

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after, fixTitle: "Add OnPropertyChanged()");
        }

        [Test]
        public static void TrySetOnly()
        {
            var before = @"
namespace N.Client
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p;

        ↓public event PropertyChangedEventHandler? PropertyChanged;

        public string P
        {
            get { return this.p; }
            set { this.TrySet(ref this.p, value); }
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}";

            var after = @"
namespace N.Client
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P
        {
            get { return this.p; }
            set { this.TrySet(ref this.p, value); }
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Add OnPropertyChanged()");
        }
    }
}
