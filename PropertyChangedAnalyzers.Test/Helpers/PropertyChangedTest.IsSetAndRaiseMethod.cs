namespace PropertyChangedAnalyzers.Test
{
    using System.Linq;
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    internal partial class PropertyChangedTest
    {
        internal class IsSetAndRaiseMethod
        {
            [Test]
            public void Stylet()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int value;

        public int Value
        {
            get { return value; }
            set { this.SetAndNotify(ref this.value, value); }
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes().Concat(new[] { SpecialMetadataReferences.Stylet }));
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("SetAndNotify");
                var method = (IMethodSymbol)semanticModel.GetSymbolSafe(invocation, CancellationToken.None);
                Assert.AreEqual(true, PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void CaliburnMicro()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        private int value;

        public int Value
        {
            get { return value; }
            set { this.Set(ref this.value, value); }
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase).Assembly));
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("Set");
                var method = (IMethodSymbol)semanticModel.GetSymbolSafe(invocation, CancellationToken.None);
                Assert.AreEqual(true, PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void MvvmLight()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        private int value;

        public int Value
        {
            get { return value; }
            set { this.Set(ref this.value, value); }
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.Transitive(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly));
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("Set");
                var method = (IMethodSymbol)semanticModel.GetSymbolSafe(invocation, CancellationToken.None);
                Assert.AreEqual(true, PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void CustomImplementation1()
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

        protected bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
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
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var methodDeclaration = syntaxTree.FindMethodDeclaration("SetValue");
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
                Assert.AreEqual(true, PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void CustomImplementation2()
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

        protected virtual bool SetValue<T>(ref T field, T value, Action OnChanging = null, Action OnChanged = null, [CallerMemberName]string propertyName = null)
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
                var methodDeclaration = syntaxTree.FindMethodDeclaration("SetValue");
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
                Assert.AreEqual(true, PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void OverridingCaliburnMicroPropertyChangedBase()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    public abstract class FooBase : Caliburn.Micro.PropertyChangedBase
    {
        public override bool Set<T>(ref T oldValue, T newValue, string propertyName = null)
        {
            return base.Set(ref oldValue, newValue, propertyName);
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes().Concat(MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase).Assembly)));
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var methodDeclaration = syntaxTree.FindMethodDeclaration("Set");
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
                Assert.AreEqual(true, PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void CallingCaliburnMicroPropertyChangedBase()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    public abstract class FooBase : Caliburn.Micro.PropertyChangedBase
    {
        public bool SetValue<T>(ref T oldValue, T newValue, string propertyName = null)
        {
            return base.Set(ref oldValue, newValue, propertyName);
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes().Concat(MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase).Assembly)));
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var methodDeclaration = syntaxTree.FindMethodDeclaration("SetValue");
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
                Assert.AreEqual(true, PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void Recursive1()
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

        protected bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (this.SetValue(ref field, newValue, propertyName))
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
                var methodDeclaration = syntaxTree.FindMethodDeclaration("SetValue");
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
                Assert.AreEqual(false, PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void Recursive2()
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

        protected bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            return this.SetValue(ref field, newValue, propertyName);
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
                var methodDeclaration = syntaxTree.FindMethodDeclaration("SetValue");
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
                Assert.AreEqual(false, PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, CancellationToken.None));
            }
        }
    }
}