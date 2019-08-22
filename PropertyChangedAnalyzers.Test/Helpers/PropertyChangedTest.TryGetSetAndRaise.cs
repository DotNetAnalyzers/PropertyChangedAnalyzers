namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Linq;
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static partial class PropertyChangedTest
    {
        public static class TryGetTrySet
        {
            [Test]
            public static void CustomImplementation1()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
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
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var typeDeclaration = syntaxTree.FindClassDeclaration("ViewModelBase");
                var type = semanticModel.GetDeclaredSymbol(typeDeclaration);
                Assert.AreEqual(true, PropertyChanged.TryFindTrySet(type, semanticModel, CancellationToken.None, out var method));
                Assert.AreEqual("TrySet", method.Name);
            }

            [Test]
            public static void CustomImplementation2()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// INotifyPropertyChanged base implementation
    /// </summary>
    /// <seealso cref=""System.ComponentModel.INotifyPropertyChanged"" />
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name=""propertyName"">Name of the property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool TrySet<T>(ref T field, T value, Action OnChanging = null, Action OnChanged = null, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            OnChanging?.Invoke();

            field = value;
            OnPropertyChanged(propertyName);

            OnChanged?.Invoke();

            return true;
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var typeDeclaration = syntaxTree.FindClassDeclaration("ObservableObject");
                var type = semanticModel.GetDeclaredSymbol(typeDeclaration);
                Assert.AreEqual(true, PropertyChanged.TryFindTrySet(type, semanticModel, CancellationToken.None, out var method));
                Assert.AreEqual("TrySet", method.Name);
            }

            [Test]
            public static void OverridingCaliburnMicroPropertyChangedBase()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    public abstract class FooBase : Caliburn.Micro.PropertyChangedBase
    {
        public override bool Set<T>(ref T oldValue, T value, string propertyName = null)
        {
            return base.Set(ref oldValue, value, propertyName);
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes().Concat(MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase).Assembly)));
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var typeDeclaration = syntaxTree.FindClassDeclaration("FooBase");
                var type = semanticModel.GetDeclaredSymbol(typeDeclaration);
                Assert.AreEqual(true, PropertyChanged.TryFindTrySet(type, semanticModel, CancellationToken.None, out var method));
                Assert.AreEqual("Set", method.Name);
            }

            [Test]
            public static void CallingCaliburnMicroPropertyChangedBase()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    public abstract class FooBase : Caliburn.Micro.PropertyChangedBase
    {
        public bool TrySet<T>(ref T oldValue, T value, string propertyName = null)
        {
            return base.Set(ref oldValue, value, propertyName);
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes().Concat(MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase).Assembly)));
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var typeDeclaration = syntaxTree.FindClassDeclaration("FooBase");
                var type = semanticModel.GetDeclaredSymbol(typeDeclaration);
                Assert.AreEqual(true, PropertyChanged.TryFindTrySet(type, semanticModel, CancellationToken.None, out var method));
                Assert.AreEqual("TrySet", method.Name);
            }

            [Test]
            public static void Recursive1()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (this.TrySet(ref field, value, propertyName))
            {
                this.OnPropertyChanged(propertyName);
            }

            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var typeDeclaration = syntaxTree.FindClassDeclaration("ViewModelBase");
                var type = semanticModel.GetDeclaredSymbol(typeDeclaration);
                Assert.AreEqual(false, PropertyChanged.TryFindTrySet(type, semanticModel, CancellationToken.None, out _));
            }

            [Test]
            public static void Recursive2()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
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
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var typeDeclaration = syntaxTree.FindClassDeclaration("ViewModelBase");
                var type = semanticModel.GetDeclaredSymbol(typeDeclaration);
                Assert.AreEqual(false, PropertyChanged.TryFindTrySet(type, semanticModel, CancellationToken.None, out _));
            }
        }
    }
}
