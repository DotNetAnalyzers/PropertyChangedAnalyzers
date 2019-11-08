namespace PropertyChangedAnalyzers.Test.INPC015PropertyIsRecursiveTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC015PropertyIsRecursive;

        [Test]
        public static void NotifyingProperty()
        {
            var code = @"
namespace N
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

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void GetSetBackingFieldExpressionBodies()
        {
            var code = @"
namespace N
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ExpressionBodyReturnBase()
        {
            var code = @"
namespace N
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ExpressionBodiesGetAndSetBase()
        {
            var code = @"
namespace N
{
    public class A
    {
        public virtual int Value { get; set; }
    }

    public class B : A
    {
        public int Value
        {
            get => base.Value;
            set => base.Value = value;
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ExplicitInterfaceImplementation()
        {
            var code = @"
namespace N
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoreObjectInitializer()
        {
            var withProperties = @"
namespace ValidCode.Wrapping
{
#pragma warning disable INPC001 // The class has mutable properties and should implement INotifyPropertyChanged.
    public class WithProperties
#pragma warning restore INPC001 // The class has mutable properties and should implement INotifyPropertyChanged.
    {
        public int P1 { get; set; }
        public int P2 { get; set; }
    }
}";
            var code = @"
namespace ValidCode.Wrapping
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class WrappingProperties : INotifyPropertyChanged
    {
        private WithProperties withProperties = new WithProperties();

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1
        {
            get => this.withProperties.P1;
            set
            {
                if (value == this.withProperties.P1)
                {
                    return;
                }

                this.withProperties.P1 = value;
                this.OnPropertyChanged();
            }
        }

        public int P2
        {
            get => this.withProperties.P2;
#pragma warning disable INPC003 // Notify when property changes.
            set => this.TrySet(ref this.withProperties, new WithProperties { P1 = this.P1, P2 = this.P2 });
#pragma warning restore INPC003 // Notify when property changes.
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
    }
}";

            RoslynAssert.Valid(Analyzer, withProperties, code);
        }
    }
}
