namespace PropertyChangedAnalyzers.Test.INPC007MissingInvokerTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new EventAnalyzer();

        [Test]
        public static void OnPropertyChangedCallerMemberName()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
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
        public static void OnPropertyChangedCallerMemberNameSealed()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void OverridingEvent()
        {
            var viewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var code = @"
namespace RoslynSandbox.Client
{
    using System.ComponentModel;

    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        public override event PropertyChangedEventHandler PropertyChanged;
    }
}";

            RoslynAssert.Valid(Analyzer, viewModelBaseCode, code);
        }

        [Test]
        public static void Interface()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    internal interface IPropertyTracker
    {
        event PropertyChangedEventHandler TrackedPropertyChanged;
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void InterfaceRepro()
        {
            var code = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A view of the changes in an observable collection
    /// </summary>
    public interface IChanges<TValue> : IDisposable
    {
        /// <summary>
        /// When an item is added. On replace add is called before remove.
        /// </summary>
        event Action<TValue> Add;

        /// <summary>
        /// When an item is removed. On replace add is called before remove.
        /// </summary>
        event Action<TValue> Remove;

        /// <summary>
        /// When the collection signals reset.
        /// </summary>
        event Action<IEnumerable<TValue>> Reset;

        /// <summary>
        /// The values of the collection.
        /// </summary>
        IEnumerable<TValue> Values { get; }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void SealedWithNoMutableProperties()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public sealed class Foo : INotifyPropertyChanged
    {
        public Foo(int value )
        {
            this.Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value { get; }

        public int Squared => this.Value * this.Value;
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenCreatingPropertyChangedEventArgsSeparately()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get => this.bar;

            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                handler.Invoke(this, args);
            }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void StaticWithInvoker()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public static class Foo
    {
        public static event PropertyChangedEventHandler PropertyChanged;

        private static void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WrappingPoint()
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private Point point;

        public event PropertyChangedEventHandler PropertyChanged;

        public int X
        {
            get => this.point.X;
            set
            {
                if (value == this.point.X)
                {
                    return;
                }

                this.point = new Point(value, this.point.Y);
                this.OnPropertyChanged();
            }
        }

        public int Y
        {
            get => this.point.Y;
            set
            {
                if (value == this.point.Y)
                {
                    return;
                }

                this.point = new Point(this.point.X, value);
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
        public static void CachingInConcurrentDictionary1()
        {
            var code = @"
namespace ValidCode
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class CachingInConcurrentDictionary : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> Cache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, Cache.GetOrAdd(propertyName ?? string.Empty, name => new PropertyChangedEventArgs(name)));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void CachingInConcurrentDictionary2()
        {
            var code = @"
namespace ValidCode
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class CachingInConcurrentDictionary : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> Cache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, Cache.GetOrAdd(propertyName, name => new PropertyChangedEventArgs(name)));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void CachingInConcurrentDictionaryLocal()
        {
            var code = @"
namespace ValidCode
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class CachingInConcurrentDictionary : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> _propertyChangedCache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            var args = _propertyChangedCache.GetOrAdd(propertyName, name => new PropertyChangedEventArgs(propertyName));

            PropertyChanged?.Invoke(this, args);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
