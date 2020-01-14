namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public partial class OnPropertyChangedTests
    {
        public static class MatchMethodSymbol
        {
            [Test]
            public static void Elvis()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
                Assert.AreEqual(AnalysisResult.Yes, OnPropertyChanged.Match(method, semanticModel, CancellationToken.None)?.AnalysisResult);
            }

            [Test]
            public static void CopyLocalInvoke()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
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
                Assert.AreEqual(AnalysisResult.Yes, OnPropertyChanged.Match(method, semanticModel, CancellationToken.None)?.AnalysisResult);
            }

            [Test]
            public static void WhenCreatingPropertyChangedEventArgsSeparately()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.p;

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
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
                Assert.AreEqual(AnalysisResult.Yes, OnPropertyChanged.Match(method, semanticModel, CancellationToken.None)?.AnalysisResult);
            }

            [Test]
            public static void IgnoreWhenRaiseForOtherInstance()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
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
                Assert.AreEqual(null, OnPropertyChanged.Match(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public static void Stylet()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    public class Foo : Stylet.PropertyChangedBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.NotifyOfPropertyChange();
            }
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, SpecialMetadataReferences.Stylet);
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("NotifyOfPropertyChange");
                var method = semanticModel.GetSymbolSafe(invocation, CancellationToken.None);
                Assert.AreEqual(AnalysisResult.Yes, OnPropertyChanged.Match(method, semanticModel, CancellationToken.None)?.AnalysisResult);
            }

            [Test]
            public static void CaliburnMicro()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
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
                Assert.AreEqual(AnalysisResult.Yes, OnPropertyChanged.Match(method, semanticModel, CancellationToken.None)?.AnalysisResult);
            }

            [Test]
            public static void MvvmLight()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        private int p;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
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
                Assert.AreEqual(AnalysisResult.Yes, OnPropertyChanged.Match(method, semanticModel, CancellationToken.None)?.AnalysisResult);
            }

            [Test]
            public static void WhenNotInvoker()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    public class Foo
    {
        public Foo()
        {
            P();
        }

        private void P()
        {
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("P();");
                Assert.AreEqual(null, OnPropertyChanged.Match(invocation, semanticModel, CancellationToken.None));
            }

            [TestCase("P1()", AnalysisResult.No)]
            [TestCase("P2()", AnalysisResult.No)]
            [TestCase("P3()", AnalysisResult.No)]
            [TestCase("P4()", AnalysisResult.No)]
            [TestCase("OnPropertyChanged();", AnalysisResult.Yes)]
            public static void WhenNotInvokerINotifyPropertyChangedFullyQualified(string call, AnalysisResult expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        public Foo()
        {
            P1();
            var a = P2();
            a = P3();
            if (P4())
            {
            }

            OnPropertyChanged();
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        private void P1()
        {
        }

        private int P2() => 1;

        private int P3() => 2;

        private bool P4() => true;
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation(call);
                if (expected == AnalysisResult.No)
                {
                    Assert.AreEqual(null, OnPropertyChanged.Match(invocation, semanticModel, CancellationToken.None));
                }
                else
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    Assert.AreEqual(expected, OnPropertyChanged.Match(invocation, semanticModel, CancellationToken.None).Value.AnalysisResult);
                }
            }

            [TestCase("protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)")]
            [TestCase("protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)")]
            [TestCase("protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)")]
            public static void WhenTrue(string signature)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
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
                Assert.AreEqual(AnalysisResult.Yes, OnPropertyChanged.Match(method, semanticModel, CancellationToken.None)?.AnalysisResult);
            }

            [TestCase("protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)")]
            [TestCase("protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)")]
            [TestCase("protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)")]
            public static void WhenRecursive(string signature)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
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
                Assert.AreEqual(null, OnPropertyChanged.Match(method, semanticModel, CancellationToken.None));
            }

            [Test]
            public static void ExceptionHandlingRelayCommand()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
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
                Assert.AreEqual(AnalysisResult.Maybe, OnPropertyChanged.Match(invocation, semanticModel, CancellationToken.None).Value.AnalysisResult);
            }
        }
    }
}
