#pragma warning disable GURA04,GURA06 // Move test to correct class.
namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    public static class ValidWithAllAnalyzers
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers = typeof(Descriptors)
            .Assembly
            .GetTypes()
            .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
            .ToArray();

        private static readonly Solution AnalyzersProjectSolution = CodeFactory.CreateSolution(
            ProjectFile.Find("PropertyChangedAnalyzers.csproj"),
            AllAnalyzers,
            MetadataReferences.FromAttributes());

        private static readonly Solution ValidCodeProjectSln = CodeFactory.CreateSolution(
            ProjectFile.Find("ValidCode.csproj"),
            AllAnalyzers,
            MetadataReferences.FromAttributes());

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
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var barCode = @"
namespace N
{
    public class WithMutableField
    {
        public int F;
    }
}";

            var viewModel1Code = @"
namespace N.Client
{
    using N.Core;

    public class ViewModel1 : ViewModelBase
    {
        private int value;
        private int value2;

        public int Sum => this.Value + this.Value2;

        public int Value
        {
#pragma warning disable INPC020 // Prefer expression body accessor.
            get
            {
                return this.value;
            }
#pragma warning restore INPC020 // Prefer expression body accessor.
            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(Sum));
            }
        }

        public int Value2
        {
            get => this.value2;
            set
            {
                if (this.TrySet(ref this.value2, value))
                {
                    this.OnPropertyChanged(nameof(this.Sum));
                }
            }
        }
    }
}";

            var viewModel2Code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel2 : INotifyPropertyChanged
    {
        private int value;
        private readonly WithMutableField p = new WithMutableField();

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => this.Value * this.Value;

        public int Value
        {
#pragma warning disable INPC020 // Prefer expression body accessor.
            get
            {
                return this.value;
            }
#pragma warning restore INPC020 // Prefer expression body accessor.
            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(Squared));
            }
        }

        public int P2
        {
            get => this.p.F;
            set
            {
                if (value == this.p.F)
                {
                    return;
                }

                this.p.F = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

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
            if (handler != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                handler.Invoke(this, args);
            }
        }
    }
}";

            var c2 = @"
namespace N
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var c3 = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C3 : INotifyPropertyChanged
    {
        private TimeSpan timeSpan;

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var foo3Code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo3 : INotifyPropertyChanged
    {
        private double speed;

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var foo4Code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class Foo4 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract int Value { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var foo5Code = @"
namespace N
{
    public class Foo5 : Foo4
    {
        private int value;

        public override int Value => this.value;

        public void Update(int value)
        {
            this.value = value;
            this.OnPropertyChanged(nameof(this.Value));
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
        private Exception _exception;

        public ExceptionHandlingRelayCommand(Action action, ICondition condition)
            : base(action, condition)
        {
        }

        public Exception Exception
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
                viewModelBaseCode,
                barCode,
                viewModel1Code,
                viewModel2Code,
                c1,
                c2,
                c3,
                foo3Code,
                foo4Code,
                foo5Code,
                exceptionHandlingRelayCommand);
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

    public abstract class ViewModelBase<_> : INotifyPropertyChanged
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
            var viewModel1 = @"
namespace N.Client
{
    using N.Core;

    public class ViewModel1 : ViewModelBase<int>
    {
        private int value;
        private int value2;

        public int Sum => this.Value + this.Value2;

        public int Value
        {
#pragma warning disable INPC020 // Prefer expression body accessor.
            get
            {
                return this.value;
            }
#pragma warning restore INPC020 // Prefer expression body accessor.
            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(Sum));
            }
        }

        public int Value2
        {
            get => this.value2;
            set
            {
                if (this.TrySet(ref this.value2, value))
                {
                    this.OnPropertyChanged(nameof(this.Sum));
                }
            }
        }
    }
}";

            var viewModelOfT = @"
namespace N
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel<T> : INotifyPropertyChanged
    {
        private T value;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Text => $""{this.Value}  {this.Value}"";

        public T Value
        {
#pragma warning disable INPC020 // Prefer expression body accessor.
            get
            {
                return this.value;
            }
#pragma warning restore INPC020 // Prefer expression body accessor.

            set
            {
                if (EqualityComparer<T>.Default.Equals(value, this.value))
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(Text));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(analyzer, viewModelBaseOf_, viewModel1, viewModelOfT);
        }
    }
}
