namespace PropertyChangedAnalyzers.Test.INPC007MissingInvoker
{
    using Gu.Roslyn.Asserts;

    using NUnit.Framework;

    public static class Valid
    {
        private static readonly EventAnalyzer Analyzer = new();

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
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class C : INotifyPropertyChanged
    {
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
namespace N.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var code = @"
namespace N.Client
{
    using System.ComponentModel;

    public class C : N.Core.ViewModelBase
    {
        public override event PropertyChangedEventHandler? PropertyChanged;
    }
}";

            RoslynAssert.Valid(Analyzer, new[] { viewModelBaseCode, code }, settings: Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.Warnings));
        }

        [Test]
        public static void Interface()
        {
            var code = @"
namespace N
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
namespace N
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

            RoslynAssert.Valid(Analyzer, code, settings: Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.Warnings));
        }

        [Test]
        public static void WhenCreatingPropertyChangedEventArgsSeparately()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public static class C
    {
        public static event PropertyChangedEventHandler? PropertyChanged;

        private static void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
namespace N
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private Point point;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, Cache.GetOrAdd(propertyName ?? string.Empty, name => new PropertyChangedEventArgs(name)));
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string? propertyName = null)
        {
            var args = _propertyChangedCache.GetOrAdd(propertyName ?? string.Empty, name => new PropertyChangedEventArgs(propertyName));

            PropertyChanged?.Invoke(this, args);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
