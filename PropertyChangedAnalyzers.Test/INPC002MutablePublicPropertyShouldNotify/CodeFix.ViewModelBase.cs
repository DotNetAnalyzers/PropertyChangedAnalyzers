namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class ViewModelBase
        {
            private const string ViewModelBaseCode = @"
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

            private const string ViewModelBaseUnderscore = @"
namespace N
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
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            [Test]
            public static void AutoPropertyToNotifyWhenValueChanges()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

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
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "Notify when value changes.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "Notify when value changes.");
            }

            [Test]
            public static void AutoPropertyToTrySet()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P { get => this.p; set => this.TrySet(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void AutoPropertyInitializedToSet()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public int ↓P { get; set; } = 1;
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p = 1;

        public int P { get => this.p; set => this.TrySet(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void AutoPropertyVirtualToSet()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public virtual int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public virtual int P { get => this.p; set => this.TrySet(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void AutoPropertyPrivateSetToSet()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public int ↓P { get; private set; }

        public void Mutate()
        {
            this.P++;
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P { get => this.p; private set => this.TrySet(ref this.p, value); }

        public void Mutate()
        {
            this.P++;
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void AutoPropertyToTrySetUnderscoreNames()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public C(int p)
        {
            P = p;
        }

        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int _p;

        public C(int p)
        {
            P = p;
        }

        public int P { get => _p; set => TrySet(ref _p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Code.UnqualifiedUnderscoreFields, ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void WithBackingFieldToSetStatementBody()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string ↓P
        {
            get { return this.p; }
            set { this.p = value; }
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string P
        {
            get { return this.p; }
            set { this.TrySet(ref this.p, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void WithBackingFieldToSetExpressionBody()
            {
                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string ↓P
        {
            get => this.p;
            set => this.p = value;
        }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p;

        public string P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesStatementBody()
            {
                var before = @"
namespace N
{
    public class C : ViewModelBase
    {
        private string _p;

        public string ↓P
        {
            get { return _p; }
            set { _p = value; }
        }
    }
}";

                var after = @"
namespace N
{
    public class C : ViewModelBase
    {
        private string _p;

        public string P
        {
            get { return _p; }
            set { TrySet(ref _p, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void WithBackingFieldToSetUnderscoreNamesExpressionBody()
            {
                var before = @"
namespace N
{
    public class C : N.ViewModelBase
    {
        private string _p;

        public string ↓P
        {
            get => _p;
            set => _p = value;
        }
    }
}";

                var after = @"
namespace N
{
    public class C : N.ViewModelBase
    {
        private string _p;

        public string P
        {
            get => _p;
            set => TrySet(ref _p, value);
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBaseUnderscore, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void AutoPropertyWhenRecursionInTrySet()
            {
                var viewModelBaseCode = @"
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
            return this.TrySet(ref field, value, propertyName);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

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
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after);
            }

            [Test]
            public static void AutoPropertyWhenNullCoalescingInTrySet()
            {
                var viewModelBaseCode = @"
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
            this.OnPropertyChanged(propertyName ?? string.Empty);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public int P { get => this.p; set => this.TrySet(ref this.p, value); }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after, fixTitle: "TrySet(ref field, value)");
            }

            [Test]
            public static void UglyViewModelBase()
            {
                var c1 = @"
namespace N
{
    public class C1
    {
        public int P;
    }
}";

                var viewModelBaseCode = @"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace MVVM
{
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        private string displayName;
        #region Constructor

        protected ViewModelBase()
        {
        }

        #endregion // Constructor

        #region DisplayName

        /// <summary>
        /// Returns the user-friendly name of this object.
        /// Child classes can set this property to a new value,
        /// or override it to determine the value on-demand.
        /// </summary>
        public virtual string DisplayName
        {
            get => this.displayName;
            protected set
            {
                if (value == this.displayName)
                {
                    return;
                }

                this.displayName = value;
                this.NotifyPropertyChanged(nameof(DisplayName));
            }
        }

        #endregion // DisplayName

        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional(""DEBUG"")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = ""Invalid property name: "" + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name=""propertyName"">The property that has a new value.</param>
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        protected virtual void NotifyPropertyChangedAll(object inOjbect)
        {
            foreach (PropertyInfo pi in inOjbect.GetType().GetProperties())
            {
                NotifyPropertyChanged(pi.Name);
            }
        }
        public virtual void Refresh()
        {
            NotifyPropertyChangedAll(this);
        }
        #endregion // INotifyPropertyChanged Members

        #region IDisposable Members
        readonly List<IDisposable> _disposables = new List<IDisposable>();
        readonly object _disposeLock = new object();
        bool _isDisposed;

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            lock (_disposeLock)
            {
                this.OnDispose();

                if (_isDisposed) return;

                foreach (var disposable in _disposables)
                    disposable.Dispose();

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

#if DEBUG
        /// <summary>
        /// Useful for ensuring that ViewModel objects are properly garbage collected.
        /// </summary>
        ~ViewModelBase()
        {
            string msg = string.Format(""{0} ({1}) ({2}) Finalized"", this.GetType().Name, this.DisplayName, this.GetHashCode());
            System.Diagnostics.Debug.WriteLine(msg);
        }
#endif

        #endregion // IDisposable Members
    }
}";
                var before = @"
namespace N
{
    using MVVM;

    public class C : ViewModelBase
    {
        private readonly C1 c1 = new C1();

        public int ↓P
        {
            get => this.c1.P;
            set => this.c1.P = value;
        }
    }
}";

                var after = @"
namespace N
{
    using MVVM;

    public class C : ViewModelBase
    {
        private readonly C1 c1 = new C1();

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
                this.NotifyPropertyChanged(nameof(this.P));
            }
        }
    }
}";

                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, c1, before }, after, fixTitle: "Notify when value changes.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, c1, before }, after, fixTitle: "Notify when value changes.");
            }

            [Test]
            public static void ViewModelBaseWithPropertyChangedEventArgsParameter()
            {
                var viewModelBase = @"
namespace N
{
    using System.ComponentModel;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
                var before = @"
namespace N
{
    public class C : ViewModelBase
    {
        public int ↓P { get; set; }
    }
}";

                var after = @"
namespace N
{
    public class C : ViewModelBase
    {
        private int p;

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
                this.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(this.P)));
            }
        }
    }
}";

                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBase, before }, after, fixTitle: "Notify when value changes.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBase, before }, after, fixTitle: "Notify when value changes.");
            }
        }
    }
}
