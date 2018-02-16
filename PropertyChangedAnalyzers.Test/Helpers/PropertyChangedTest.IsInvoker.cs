namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Linq;
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    internal partial class PropertyChangedTest
    {
        internal class IsInvoker
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
        private int bar;

        public int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.NotifyOfPropertyChange();
            }
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes().Concat(new[] { SpecialMetadataReferences.Stylet }));
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("NotifyOfPropertyChange");
                var method = (IMethodSymbol)semanticModel.GetSymbolSafe(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsInvoker(method, semanticModel, CancellationToken.None));
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
        private int bar;

        public int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.NotifyOfPropertyChange();
            }
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.Transitive(typeof(Caliburn.Micro.PropertyChangedBase).Assembly));
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("NotifyOfPropertyChange");
                var method = (IMethodSymbol)semanticModel.GetSymbolSafe(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsInvoker(method, semanticModel, CancellationToken.None));
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
        private int bar;

        public int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.RaisePropertyChanged();
            }
        }
    }
}");
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.Transitive(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly));
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("RaisePropertyChanged");
                var method = (IMethodSymbol)semanticModel.GetSymbolSafe(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsInvoker(method, semanticModel, CancellationToken.None));
            }

            [TestCase("protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)")]
            [TestCase("protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)")]
            [TestCase("protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)")]
            public void WhenTrue(string signature)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var methodDeclaration = syntaxTree.FindMethodDeclaration(signature);
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsInvoker(method, semanticModel, CancellationToken.None));
            }

            [TestCase("protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)")]
            [TestCase("protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)")]
            [TestCase("protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)")]
            public void WhenRecursive(string signature)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(property);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
             this.OnPropertyChanged(e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanged(propertyName);
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var methodDeclaration = syntaxTree.FindMethodDeclaration(signature);
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
                Assert.AreEqual(AnalysisResult.No, PropertyChanged.IsInvoker(method, semanticModel, CancellationToken.None));
            }
        }
    }
}