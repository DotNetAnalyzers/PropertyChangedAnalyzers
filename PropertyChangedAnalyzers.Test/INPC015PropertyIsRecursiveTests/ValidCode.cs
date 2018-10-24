namespace PropertyChangedAnalyzers.Test.INPC015PropertyIsRecursiveTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = INPC015PropertyIsRecursive.Descriptor;

        [Test]
        public void NotifyingProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged(nameof(Value));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, Descriptor, testCode);
        }

        [Test]
        public void GetSetExpressionBodyAccessors()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo
    {
        private int value;

        public int Value
        {
            get => this.value;
            set => this.value = value;
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void BaseCall()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class A
    {
        public virtual int Value => 1;
    }

    public class B : A
    {
        public override int Value => base.Value;
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void ExplicitInterfaceImplementation()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections;
    using System.Collections.Generic;

    public class Foo : IReadOnlyList<int>
    {
        private readonly List<int> ints;
        
        public int Count => ints.Count;

        int IReadOnlyCollection<int>.Count => this.Count;

        public int this[int index] => ints[index];

        public IEnumerator<int> GetEnumerator()
        {
            return ints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) ints).GetEnumerator();
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
