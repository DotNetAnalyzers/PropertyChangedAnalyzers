namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    internal partial class PropertyChangedTest
    {
        internal class TryGetOnPropertyChanged
        {
            [Test]
            public void ElvisCallerMemberName()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
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
                var classDeclaration = syntaxTree.FindClassDeclaration("Foo");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(true, PropertyChanged.TryGetOnPropertyChanged(type, semanticModel, CancellationToken.None, out var invoker));
                Assert.AreEqual("RoslynSandbox.Foo.OnPropertyChanged(string)", invoker.ToString());
            }

            [Test]
            public void CopyLocalNullCheckCallerMemberName()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
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
                var classDeclaration = syntaxTree.FindClassDeclaration("Foo");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(true, PropertyChanged.TryGetOnPropertyChanged(type, semanticModel, CancellationToken.None, out var invoker));
                Assert.AreEqual("RoslynSandbox.Foo.OnPropertyChanged(string)", invoker.ToString());
            }

            [Test]
            public void PropertyChangedEventArgsBeforeCallerMemberName()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar { get; set; }

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
                var classDeclaration = syntaxTree.FindClassDeclaration("Foo");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(true, PropertyChanged.TryGetOnPropertyChanged(type, semanticModel, CancellationToken.None, out var invoker));
                Assert.AreEqual("RoslynSandbox.Foo.OnPropertyChanged(string)", invoker.ToString());
            }

            [Test]
            public void CallerMemberNameBeforePropertyChangedEventArgs()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar { get; set; }

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
                var classDeclaration = syntaxTree.FindClassDeclaration("Foo");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(true, PropertyChanged.TryGetOnPropertyChanged(type, semanticModel, CancellationToken.None, out var invoker));
                Assert.AreEqual("RoslynSandbox.Foo.OnPropertyChanged(string)", invoker.ToString());
            }

            [Test]
            public void OverridingEvent()
            {
                var viewModelBaseCode = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");

                var testCode = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : RoslynSandbox.ViewModelBase
    {
        public override event PropertyChangedEventHandler PropertyChanged;
    }
}");

                var compilation = CSharpCompilation.Create("test", new[] { viewModelBaseCode, testCode }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(testCode);
                var classDeclaration = testCode.FindClassDeclaration("ViewModel");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(true, PropertyChanged.TryGetOnPropertyChanged(type, semanticModel, CancellationToken.None, out var invoker));
                Assert.AreEqual("RoslynSandbox.ViewModelBase.OnPropertyChanged(string)", invoker.ToString());
            }

            [Test]
            public void OverridingEventPrivateInvokerInBase()
            {
                var viewModelBaseCode = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");

                var testCode = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : RoslynSandbox.ViewModelBase
    {
        public override event PropertyChangedEventHandler PropertyChanged;
    }
}");

                var compilation = CSharpCompilation.Create("test", new[] { viewModelBaseCode, testCode }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(testCode);
                var classDeclaration = testCode.FindClassDeclaration("ViewModel");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(false, PropertyChanged.TryGetOnPropertyChanged(type, semanticModel, CancellationToken.None, out _));
            }

            [Test]
            public void Static()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public static class Foo
    {
        public static event PropertyChangedEventHandler PropertyChanged;

        private static void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classDeclaration = syntaxTree.FindClassDeclaration("Foo");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(true, PropertyChanged.TryGetOnPropertyChanged(type, semanticModel, CancellationToken.None, out var invoker));
                Assert.AreEqual("RoslynSandbox.Foo.OnPropertyChanged(string)", invoker.ToString());
            }

            [Test]
            public void Recursive()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar { get; set; }

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
                var classDeclaration = syntaxTree.FindClassDeclaration("Foo");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(false, PropertyChanged.TryGetOnPropertyChanged(type, semanticModel, CancellationToken.None, out _));
            }
        }
    }
}
