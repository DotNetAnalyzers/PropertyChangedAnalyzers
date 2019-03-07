namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    internal partial class PropertyChangedTest
    {
        internal class IsOnPropertyChanged
        {
            [Test]
            public void Elvis()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindMethodDeclaration("OnPropertyChanged");
                var method = semanticModel.GetDeclaredSymbol(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsOnPropertyChanged(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void CopyLocalInvoke()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindMethodDeclaration("OnPropertyChanged");
                var method = semanticModel.GetDeclaredSymbol(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsOnPropertyChanged(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void WhenCreatingPropertyChangedEventArgsSeparately()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

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
}");

                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindMethodDeclaration("OnPropertyChanged");
                var method = semanticModel.GetDeclaredSymbol(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsOnPropertyChanged(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void IgnoreWhenRaiseForOtherInstance()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaiseForChild(string propertyName)
        {
            var vm = new ViewModel();
            vm.OnPropertyChanged(propertyName);
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindMethodDeclaration("RaiseForChild");
                var method = semanticModel.GetDeclaredSymbol(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.No, PropertyChanged.IsOnPropertyChanged(method, semanticModel, CancellationToken.None));
            }

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
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, SpecialMetadataReferences.Stylet);
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("NotifyOfPropertyChange");
                var method = semanticModel.GetSymbolSafe(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsOnPropertyChanged(method, semanticModel, CancellationToken.None));
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
                var method = semanticModel.GetSymbolSafe(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsOnPropertyChanged(method, semanticModel, CancellationToken.None));
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
                var method = semanticModel.GetSymbolSafe(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsOnPropertyChanged(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void WhenNotInvoker()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
            Bar();
        }

        private void Bar()
        {
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("Bar();");
                Assert.AreEqual(AnalysisResult.No, PropertyChanged.IsOnPropertyChanged(invocation, semanticModel, CancellationToken.None));
            }

            [TestCase("Bar1()", AnalysisResult.No)]
            [TestCase("Bar2()", AnalysisResult.No)]
            [TestCase("Bar3()", AnalysisResult.No)]
            [TestCase("Bar4()", AnalysisResult.No)]
            [TestCase("OnPropertyChanged();", AnalysisResult.Yes)]
            public void WhenNotInvokerINotifyPropertyChangedFullyQualified(string call, AnalysisResult expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        public Foo()
        {
            Bar1();
            var a = Bar2();
            a = Bar3();
            if (Bar4())
            {
            }

            OnPropertyChanged();
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        private void Bar1()
        {
        }

        private int Bar2() => 1;

        private int Bar3() => 2;

        private bool Bar4() => true;
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation(call);
                Assert.AreEqual(expected, PropertyChanged.IsOnPropertyChanged(invocation, semanticModel, CancellationToken.None));
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
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsOnPropertyChanged(method, semanticModel, CancellationToken.None));
            }

            [TestCase("protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)")]
            [TestCase("protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)")]
            [TestCase("protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)")]
            public void WhenRecursive(string signature)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
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
                Assert.AreEqual(AnalysisResult.No, PropertyChanged.IsOnPropertyChanged(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public void ExceptionHandlingRelayCommand()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
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
                if (Equals(value, _exception))
                {
                    return;
                }

                _exception = value;
                OnPropertyChanged();
            }
        }
    }
}");

                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("OnPropertyChanged()");
                Assert.AreEqual(AnalysisResult.Maybe, PropertyChanged.IsOnPropertyChanged(invocation, semanticModel, CancellationToken.None, out var method));
                Assert.AreEqual("Gu.Wpf.Reactive.CommandBase<object>.OnPropertyChanged(string)", method.ToString());
            }
        }
    }
}
