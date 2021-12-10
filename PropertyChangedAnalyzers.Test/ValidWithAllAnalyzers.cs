namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Gu.Roslyn.Asserts;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    using PropertyChangedAnalyzers.Test.Helpers;

    public static class ValidWithAllAnalyzers
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers =
            typeof(Descriptors)
            .Assembly
            .GetTypes()
            .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
            .ToArray();

        private static readonly Solution AnalyzersProjectSolution = CodeFactory.CreateSolution(
            ProjectFile.Find("PropertyChangedAnalyzers.csproj"));

        private static readonly Solution ValidCodeProjectSln = CodeFactory.CreateSolution(
            ProjectFile.Find("ValidCode.csproj"));

        [Test]
        public static void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(AllAnalyzers);
            Assert.Pass($"Count: {AllAnalyzers.Count}");
        }

        [Ignore("Does not pick up nullable attributes.")]
        [TestCaseSource(nameof(AllAnalyzers))]
        public static void AnalyzersProject(DiagnosticAnalyzer analyzer)
        {
            RoslynAssert.Valid(analyzer, AnalyzersProjectSolution);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void ValidCodeProject(DiagnosticAnalyzer analyzer)
        {
            RoslynAssert.Valid(analyzer, ValidCodeProjectSln);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void SomewhatRealisticSample(DiagnosticAnalyzer analyzer)
        {
            var viewModelBase = @"
namespace N.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var withMutableField = @"
namespace N
{
    public class WithMutableField
    {
        public int F;
    }
}";

            var viewModel1 = @"
namespace N.Client
{
    using N.Core;

    public class ViewModel1 : ViewModelBase
    {
        private int p;
        private int p2;

        public int Sum => this.P + this.P2;

        public int P
        {
#pragma warning disable INPC020 // Prefer expression body accessor.
            get
            {
                return this.p;
            }
#pragma warning restore INPC020 // Prefer expression body accessor.
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

            var viewModel2 = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel2 : INotifyPropertyChanged
    {
        private int p;
        private readonly WithMutableField withMutableField = new WithMutableField();

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Squared => this.P * this.P;

        public int P
        {
#pragma warning disable INPC020 // Prefer expression body accessor.
            get
            {
                return this.p;
            }
#pragma warning restore INPC020 // Prefer expression body accessor.
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(Squared));
            }
        }

        public int P2
        {
            get => this.withMutableField.F;
            set
            {
                if (value == this.withMutableField.F)
                {
                    return;
                }

                this.withMutableField.F = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var oldStyleOnPropertyChanged = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class OldStyleOnPropertyChanged : INotifyPropertyChanged
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

            var wrappingPoint = @"
namespace N
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    public class WrappingPoint : INotifyPropertyChanged
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

#pragma warning disable INPC003
                this.point = new Point(value, this.point.Y);
#pragma warning restore INPC003
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

#pragma warning disable INPC003
                this.point = new Point(this.point.X, value);
#pragma warning restore INPC003
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var wrappingTimeSpan = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class WrappingTimeSpan : INotifyPropertyChanged
    {
        private TimeSpan timeSpan;

        public event PropertyChangedEventHandler? PropertyChanged;

        public long Ticks
        {
            get => this.timeSpan.Ticks;
            set
            {
                if (value == this.timeSpan.Ticks)
                {
                    return;
                }

                this.timeSpan = TimeSpan.FromTicks(value);
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var radioButtonViewModel = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class RadioButtonViewModel : INotifyPropertyChanged
    {
        private double speed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsSpeed1
        {
            get => Math.Abs(this.speed - 1) < 1E-2;
            set => this.Speed = 1;
        }

        public double Speed
        {
            get => this.speed;

            set
            {
                if (value.Equals(this.speed))
                {
                    return;
                }

                this.speed = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsSpeed1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var abstractWithAbstractProperty = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class AbstractWithAbstractProperty : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public abstract int P { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var subClassingAbstractWithAbstractProperty = @"
namespace N
{
    public class SubClassingAbstractWithAbstractProperty : AbstractWithAbstractProperty
    {
        private int p;

        public override int P => this.p;

        public void M(int p)
        {
            this.p = p;
            this.OnPropertyChanged(nameof(this.P));
        }
    }
}";

            var exceptionHandlingRelayCommand = @"
namespace N
{
    using System;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;

    public class ExceptionHandlingRelayCommand : ConditionRelayCommand
    {
        private Exception? _exception;

        public ExceptionHandlingRelayCommand(Action action, ICondition condition)
            : base(action, condition)
        {
        }

        public Exception? Exception
        {
            get => _exception;

            private set
            {
#pragma warning disable INPC006_b
                if (ReferenceEquals(value, _exception))
#pragma warning restore INPC006_b
                {
                    return;
                }

                _exception = value;
                OnPropertyChanged();
            }
        }
    }
}";
            RoslynAssert.Valid(
                analyzer,
                new[]
                {
                    viewModelBase,
                    withMutableField,
                    viewModel1,
                    viewModel2,
                    oldStyleOnPropertyChanged,
                    wrappingPoint,
                    wrappingTimeSpan,
                    radioButtonViewModel,
                    abstractWithAbstractProperty,
                    subClassingAbstractWithAbstractProperty,
                    exceptionHandlingRelayCommand,
                },
                settings: LibrarySettings.Reactive);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void SomewhatRealisticSampleGeneric(DiagnosticAnalyzer analyzer)
        {
            var viewModelBaseOf_ = @"
namespace N.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool TrySet<U>(ref U field, U value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<U>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var viewModel1 = @"
namespace N.Client
{
    using N.Core;

    public class ViewModel1 : ViewModelBase<int>
    {
        private int p;
        private int p2;

        public int Sum => this.P + this.P2;

        public int P
        {
#pragma warning disable INPC020 // Prefer expression body accessor.
            get
            {
                return this.p;
            }
#pragma warning restore INPC020 // Prefer expression body accessor.
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

            var genericViewModelOfT = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class GenericViewModel<T> : INotifyPropertyChanged
    {
        private T? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Text => $""{this.P}  {this.P}"";

        public T? P
        {
#pragma warning disable INPC020 // Prefer expression body accessor.
            get
            {
                return this.p;
            }
#pragma warning restore INPC020 // Prefer expression body accessor.

            set
            {
                if (EqualityComparer<T>.Default.Equals(value, this.p))
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(Text));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(analyzer, viewModelBaseOf_, viewModel1, genericViewModelOfT);
        }
    }
}
