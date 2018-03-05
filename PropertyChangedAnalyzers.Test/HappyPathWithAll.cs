namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    public class HappyPathWithAll
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers = typeof(AnalyzerConstants)
            .Assembly
            .GetTypes()
            .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
            .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
            .ToArray();

        private static readonly Solution Solution = CodeFactory.CreateSolution(
            CodeFactory.FindSolutionFile("PropertyChangedAnalyzers.sln"),
            AllAnalyzers,
            AnalyzerAssert.MetadataReferences);

        private static readonly Solution PropertyChangedAnalyzersProjectSln = CodeFactory.CreateSolution(
            CodeFactory.FindProjectFile("PropertyChangedAnalyzers.Analyzers.csproj"),
            AllAnalyzers,
            AnalyzerAssert.MetadataReferences);

        [Test]
        public void NotEmpty()
        {
            CollectionAssert.IsNotEmpty(AllAnalyzers);
            Assert.Pass($"Count: {AllAnalyzers.Count}");
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void PropertyChangedAnalyzersSln(DiagnosticAnalyzer analyzer)
        {
            AnalyzerAssert.Valid(analyzer, Solution);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void PropertyChangedAnalyzersProject(DiagnosticAnalyzer analyzer)
        {
            AnalyzerAssert.Valid(analyzer, PropertyChangedAnalyzersProjectSln);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void SomewhatRealisticSample(DiagnosticAnalyzer analyzer)
        {
            var viewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var viewModelSubclassCode = @"
namespace RoslynSandbox.Client
{
    using RoslynSandbox.Core;

    public class ViewModel1 : ViewModelBase
    {
        private int value;
        private int value2;

        public int Sum => this.Value + this.Value2;

        public int Value
        {
            get
            {
                return this.value;
            }

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

            var viewModelCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Squared => this.Value * this.Value;

        public int Value
        {
            get
            {
                return this.value;
            }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.Valid(analyzer, viewModelBaseCode, viewModelSubclassCode, viewModelCode);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void SomewhatRealisticSampleGeneric(DiagnosticAnalyzer analyzer)
        {
            var viewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase<_> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            var viewModelSubclassCode = @"
namespace RoslynSandbox.Client
{
    using RoslynSandbox.Core;

    public class ViewModel1 : ViewModelBase<int>
    {
        private int value;
        private int value2;

        public int Sum => this.Value + this.Value2;

        public int Value
        {
            get
            {
                return this.value;
            }

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

            var viewModelCode = @"
namespace RoslynSandbox
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
            get
            {
                return this.value;
            }

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
            AnalyzerAssert.Valid(analyzer, viewModelBaseCode, viewModelSubclassCode, viewModelCode);
        }
    }
}
