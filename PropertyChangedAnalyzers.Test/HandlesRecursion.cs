namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class HandlesRecursion
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers =
            typeof(Descriptors)
            .Assembly
            .GetTypes()
            .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
            .ToArray();

        [Test]
        public static void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(AllAnalyzers);
            Assert.Pass($"Count: {AllAnalyzers.Count}");
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static async Task InTrySet(DiagnosticAnalyzer analyzer)
        {
            var viewModelBase = @"
namespace N.Core
{
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

            var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p2;

        public int P1 { get; set; }

        public int P2
        {
            get { return this.p2; }
            set { this.TrySet(ref this.p2, value); }
        }
    }
}";
            await Analyze.GetDiagnosticsAsync(analyzer, new[] { viewModelBase, code }, MetadataReferences.FromAttributes()).ConfigureAwait(false);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static async Task InOnPropertyChanged(DiagnosticAnalyzer analyzer)
        {
            var viewModelBaseCode = @"
namespace N.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanged(propertyName);
        }
    }
}";

            var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p2;

        public int P1 { get; set; }

        public int P2
        {
            get
            {
                return this.p2;
            }

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
            }
        }
    }
}";
            await Analyze.GetDiagnosticsAsync(analyzer, new[] { viewModelBaseCode, code }, MetadataReferences.FromAttributes()).ConfigureAwait(false);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static async Task InProperty(DiagnosticAnalyzer analyzer)
        {
            var fooCode = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    public class C
    {
        private Point point;
        private double h1;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.P1;

        public int P2 => P2;

        public int P3 => this.P1;

        public int P4
        {
            get
            {
                return this.P4;
            }

            set
            {
                if (value == this.P4)
                {
                    return;
                }

                this.P4 = value;
                this.OnPropertyChanged();
            }
        }

        public int P5
        {
            get => this.P5;
            set
            {
                if (value == this.P5)
                {
                    return;
                }

                this.P5 = value;
                this.OnPropertyChanged();
            }
        }

        public int P6
        {
            get => this.P5;
            set
            {
                if (value == this.P5)
                {
                    return;
                }

                this.P5 = value;
                this.OnPropertyChanged();
            }
        }

        public int X
        {
            get => this.X;
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
            get
            {
                return this.Y;
            }

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

        public double H1
        {
            get => this.h1;
            set
            {
                if (value.Equals(this.h1))
                {
                    return;
                }

                this.h1 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Height));
                this.OnPropertyChanged(nameof(this.Height2));
            }
        }

        public double Height
        {
            get
            {
                return this.Height;
            }
        }

        public double Height2
        {
            get
            {
                return Math.Min(this.Height2, this.H1);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            await Analyze.GetDiagnosticsAsync(analyzer, new[] { fooCode }, MetadataReferences.FromAttributes()).ConfigureAwait(false);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void Repro(DiagnosticAnalyzer analyzer)
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class C : INotifyPropertyChanged
    {
        private double h1;

        public event PropertyChangedEventHandler PropertyChanged;

        public double H1
        {
            get => this.h1;
            set
            {
                if (value.Equals(this.h1))
                {
                    return;
                }

                this.h1 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Height));
            }
        }

        public double Height => Math.Min(this.Height, this.H1);

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(analyzer, code);
        }
    }
}
