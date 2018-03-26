namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class HandlesRecursion
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers = typeof(AnalyzerConstants)
            .Assembly
            .GetTypes()
            .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
            .ToArray();

        [Test]
        public void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(AllAnalyzers);
            Assert.Pass($"Count: {AllAnalyzers.Count}");
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public async Task InSetAndRaise(DiagnosticAnalyzer analyzer)
        {
            var viewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            return this.TrySet(ref field, newValue, propertyName);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int value2;

        public int Value1 { get; set; }

        public int Value2
        {
            get { return this.value2; }
            set { this.TrySet(ref this.value2, value); }
        }
    }
}";
            await Analyze.GetDiagnosticsAsync(analyzer, new[] { viewModelBaseCode, testCode }, AnalyzerAssert.MetadataReferences).ConfigureAwait(false);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public async Task InOnPropertyChanged(DiagnosticAnalyzer analyzer)
        {
            var viewModelBaseCode = @"
namespace RoslynSandbox.Core
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

            var testCode = @"
namespace RoslynSandbox.Client
{
    public class Foo : RoslynSandbox.Core.ViewModelBase
    {
        private int value2;

        public int Value1 { get; set; }

        public int Value2
        {
            get
            {
                return this.value2;
            }

            set
            {
                if (value == this.value2)
                {
                    return;
                }

                this.value2 = value;
                this.OnPropertyChanged();
            }
        }
    }
}";
            await Analyze.GetDiagnosticsAsync(analyzer, new[] { viewModelBaseCode, testCode }, AnalyzerAssert.MetadataReferences).ConfigureAwait(false);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public async Task InProperty(DiagnosticAnalyzer analyzer)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Value1 => this.Value1;

        public int Value2 => Value2;

        public int Value3 => this.Value1;

        public int Value4
        {
            get
            {
                return this.Value4;
            }

            set
            {
                if (value == this.Value4)
                {
                    return;
                }

                this.Value4 = value;
                this.OnPropertyChanged();
            }
        }

        public int Value5
        {
            get => this.Value5;
            set
            {
                if (value == this.Value5)
                {
                    return;
                }

                this.Value5 = value;
                this.OnPropertyChanged();
            }
        }

        public int Value6
        {
            get => this.Value5;
            set
            {
                if (value == this.Value5)
                {
                    return;
                }

                this.Value5 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            await Analyze.GetDiagnosticsAsync(analyzer, new[] { testCode }, AnalyzerAssert.MetadataReferences).ConfigureAwait(false);
        }
    }
}
