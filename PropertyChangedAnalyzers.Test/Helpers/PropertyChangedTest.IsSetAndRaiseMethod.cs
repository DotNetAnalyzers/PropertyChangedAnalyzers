namespace PropertyChangedAnalyzers.Test
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    internal partial class PropertyChangedTest
    {
        internal class IsSetAndRaiseMethod
        {
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

        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
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
                var methodDeclaration = syntaxTree.FindBestMatch<MethodDeclarationSyntax>("Set");
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
                var methodDeclaration = syntaxTree.FindBestMatch<MethodDeclarationSyntax>("SetValue");
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
                Assert.AreEqual(true, PropertyChanged.IsSetAndRaiseMethod(method, semanticModel, CancellationToken.None));
            }
        }
    }
}