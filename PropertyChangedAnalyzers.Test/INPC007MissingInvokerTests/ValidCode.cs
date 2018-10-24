namespace PropertyChangedAnalyzers.Test.INPC007MissingInvokerTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new INPC007MissingInvoker();

        [Test]
        public void OnPropertyChangedCallerMemberName()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void OnPropertyChangedCallerMemberNameSealed()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void OverridingEvent()
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

            var testCode = @"
namespace RoslynSandbox.Client
{
    using System.ComponentModel;

    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        public override event PropertyChangedEventHandler PropertyChanged;
    }
}";

            AnalyzerAssert.Valid(Analyzer, viewModelBaseCode, testCode);
        }

        [Test]
        [Explicit("Not sure how we want this.")]
        public void Set()
        {
            var testCode = @"
namespace RoslynSandbox.Client
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return this.name; }
            set { this.TrySet(ref this.name, value); }
        }

        protected bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void Interface()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    internal interface IPropertyTracker
    {
        event PropertyChangedEventHandler TrackedPropertyChanged;
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void InterfaceRepro()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void SealedWithNoMutableProperties()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenCreatingPropertyChangedEventArgsSeparately()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void StaticWithInvoker()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WrappingPoint()
        {
            var testCode = @"
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
