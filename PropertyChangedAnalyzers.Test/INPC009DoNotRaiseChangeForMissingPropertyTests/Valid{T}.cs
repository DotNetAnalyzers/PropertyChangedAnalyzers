namespace PropertyChangedAnalyzers.Test.INPC009DoNotRaiseChangeForMissingPropertyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [TestFixture(typeof(ArgumentAnalyzer))]
    [TestFixture(typeof(InvocationAnalyzer))]
    public static class Valid<T>
        where T : DiagnosticAnalyzer, new()
    {
        private static readonly T Analyzer = new T();
        //// ReSharper disable once StaticMemberInGenericType
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC009DoNotRaiseChangeForMissingProperty;

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""P""")]
        [TestCase(@"nameof(P)")]
        [TestCase(@"nameof(this.P)")]
        public static void OnPropertyChangedWithEventArgs(string propertyName)
        {
            var code = @"
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
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
#pragma warning disable INPC013
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
#pragma warning restore INPC013
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace(@"nameof(P)", propertyName);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void OnPropertyChangedCallerMemberName()
        {
            var code = @"
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
            get { return this.p; }
            set
            {
                if (value == this.p) return;
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void OnPropertyChangedCallerMemberNameCopyLocalNullCheckInvoke()
        {
            var code = @"
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
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void OnPropertyChangedCallerMemberNameCopyLocalNullCheckImplicitInvoke()
        {
            var code = @"
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
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""P""")]
        [TestCase(@"nameof(P)")]
        [TestCase(@"nameof(this.P)")]
        public static void Invokes(string propertyName)
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;
        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
#pragma warning disable INPC013
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.P))));
#pragma warning restore INPC013
            }
        }
    }
}".AssertReplace(@"nameof(this.P))", propertyName);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void InvokesCached()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PPropertyChangedArgs = new PropertyChangedEventArgs(nameof(P));
        private int p;
        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.PropertyChanged?.Invoke(this, PPropertyChangedArgs);
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void OnPropertyChangedInBaseClass()
        {
            var viewModelBase = @"
namespace N
{
    using System.ComponentModel;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, viewModelBase, code);
        }

        [Test]
        public static void RaisesForIndexer()
        {
            var code = @"
namespace N
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : Collection<int>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected override void SetItem(int index, int item)
        {
            base.SetItem(index, item);
            this.OnPropertyChanged(""Item[]"");
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresWhenNotInvokingReproIssue122()
        {
            var propertyChangedEventArgsExt = @"
namespace N
{
    using System.ComponentModel;

    public static class PropertyChangedEventArgsExt
    {
        public static bool HasPropertyChanged(this PropertyChangedEventArgs e, string propertyName)
        {
            return string.Equals(e.PropertyName, propertyName);
        }
    }
}";

            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public C()
        {
            this.PropertyChanged += OnPropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

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

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.HasPropertyChanged(""SomeProperty""))
            {
                // do something
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, propertyChangedEventArgsExt, code);
        }

        [Test]
        public static void RaiseForOtherInstance()
        {
            var code = @"
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
            get
            {
                return this.p;
            }

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

        public void RaiseForChild(string propertyName)
        {
            var vm = new C();
            vm.OnPropertyChanged(propertyName);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void RaiseForOtherInstanceOfOtherType()
        {
            var c1 = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C1 : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

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

        public void OnPropertyChanged([CallerMemberName]string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var code = @"
namespace N
{
    internal class C2
    {
        public void M()
        {
            var vm = new C1();
            vm.OnPropertyChanged(""P"");
        }
    }
}";
            RoslynAssert.Valid(Analyzer, c1, code);
        }

        [Test]
        public static void RaiseForOtherInstanceOfOtherTypeWithBaseClass()
        {
            var viewModelBase = @"
namespace N
{
    using System.ComponentModel;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var c1 = @"
namespace N
{
    public class C1 : ViewModelBase
    {
        private int p;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
            }
        }
    }
}";

            var code = @"
namespace N
{
    internal class C2
    {
        public void M()
        {
            var vm = new C1();
            vm.OnPropertyChanged(""P"");
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, viewModelBase, c1, code);
        }

        [Test]
        public static void WhenNotAnInvoker()
        {
            var code = @"
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
            get { return this.p; }
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(""Missing"");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("\"\"")]
        [TestCase("string.Empty")]
        [TestCase("null")]
        public static void NotifyThatAllPropertiesChanges(string arg)
        {
            var code = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    public class C : INotifyPropertyChanged
    {
        private Dictionary<int, int> map;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => map?[1] ?? 0;

        public int P2 => map?[2] ?? 0;

        public void Update(Dictionary<int, int> newMap)
        {
            this.map = newMap;
            this.OnPropertyChanged(string.Empty);
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("string.Empty", arg);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WithViewModelBase()
        {
            var viewModelBase = @"
namespace N.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var viewModelCode = @"
namespace N.Client
{
    using N.Core;

    public class C : ViewModelBase
    {
        private int p;
        private int p2;

        public int Sum => this.P + this.P2;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(Sum));
            }
        }

        public int P2
        {
            get => this.p2;
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.Sum));
                }
            }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, viewModelBase, viewModelCode);
        }

        [Test]
        public static void OverriddenProperty()
        {
            var viewModelBase = @"
namespace N.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract int P { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var c = @"
namespace N.Client
{
    using N.Core;

    public class C : ViewModelBase
    {
        private int p;

        public override int P => this.p;

        public void Update(int value)
        {
            this.p = value;
            this.OnPropertyChanged(nameof(this.P));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, viewModelBase, c);
        }
    }
}
