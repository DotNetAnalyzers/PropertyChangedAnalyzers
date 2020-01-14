namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static partial class OnPropertyChangedTests
    {
        public static class Find
        {
            [Test]
            public static void ElvisCallerMemberName()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
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
                var classDeclaration = syntaxTree.FindClassDeclaration("C");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual("N.C.OnPropertyChanged(string)", OnPropertyChanged.Find(type, semanticModel, CancellationToken.None).ToString());
            }

            [Test]
            public static void CopyLocalNullCheckCallerMemberName()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
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
                var classDeclaration = syntaxTree.FindClassDeclaration("C");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual("N.C.OnPropertyChanged(string)", OnPropertyChanged.Find(type, semanticModel, CancellationToken.None).ToString());
            }

            [Test]
            public static void PropertyChangedEventArgsBeforeCallerMemberName()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int P { get; set; }

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
                var classDeclaration = syntaxTree.FindClassDeclaration("C");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual("N.C.OnPropertyChanged(string)", OnPropertyChanged.Find(type, semanticModel, CancellationToken.None).ToString());
            }

            [Test]
            public static void CallerMemberNameBeforePropertyChangedEventArgs()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int P { get; set; }

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
                var classDeclaration = syntaxTree.FindClassDeclaration("C");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual("N.C.OnPropertyChanged(string)", OnPropertyChanged.Find(type, semanticModel, CancellationToken.None).ToString());
            }

            [Test]
            public static void OverridingEvent()
            {
                var viewModelBaseCode = CSharpSyntaxTree.ParseText(@"
namespace N
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

                var code = CSharpSyntaxTree.ParseText(@"
namespace N
{
    using System.ComponentModel;

    public class C : N.ViewModelBase
    {
        public override event PropertyChangedEventHandler PropertyChanged;
    }
}");

                var compilation = CSharpCompilation.Create("test", new[] { viewModelBaseCode, code }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(code);
                var classDeclaration = code.FindClassDeclaration("ViewModel");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual("N.ViewModelBase.OnPropertyChanged(string)", OnPropertyChanged.Find(type, semanticModel, CancellationToken.None).ToString());
            }

            [Test]
            public static void OverridingEventPrivateInvokerInBase()
            {
                var viewModelBaseCode = CSharpSyntaxTree.ParseText(@"
namespace N
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

                var code = CSharpSyntaxTree.ParseText(@"
namespace N
{
    using System.ComponentModel;

    public class C : N.ViewModelBase
    {
        public override event PropertyChangedEventHandler PropertyChanged;
    }
}");

                var compilation = CSharpCompilation.Create("test", new[] { viewModelBaseCode, code }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(code);
                var classDeclaration = code.FindClassDeclaration("ViewModel");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(null, OnPropertyChanged.Find(type, semanticModel, CancellationToken.None));
            }

            [Test]
            public static void WhenCreatingPropertyChangedEventArgsSeparately()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
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
                var classDeclaration = syntaxTree.FindClassDeclaration("C");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual("N.C.OnPropertyChanged(string)", OnPropertyChanged.Find(type, semanticModel, CancellationToken.None).ToString());
            }

            [Test]
            public static void Static()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public static class C
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
                var classDeclaration = syntaxTree.FindClassDeclaration("C");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual("N.C.OnPropertyChanged(string)", OnPropertyChanged.Find(type, semanticModel, CancellationToken.None).ToString());
            }

            [Test]
            public static void Recursive()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int P { get; set; }

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
                var classDeclaration = syntaxTree.FindClassDeclaration("C");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(null, OnPropertyChanged.Find(type, semanticModel, CancellationToken.None));
            }

            [TestCase("propertyName ?? string.Empty")]
            [TestCase("propertyName")]
            public static void CachingInConcurrentDictionary(string expression)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> Cache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, Cache.GetOrAdd(propertyName ?? string.Empty, name => new PropertyChangedEventArgs(name)));
        }
    }
}".AssertReplace("propertyName ?? string.Empty", expression));
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classDeclaration = syntaxTree.FindClassDeclaration("C");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual("N.C.OnPropertyChanged(string)", OnPropertyChanged.Find(type, semanticModel, CancellationToken.None).ToString());
            }

            [TestCase("propertyName ?? string.Empty")]
            [TestCase("propertyName")]
            public static void CachingInConcurrentDictionaryLocal(string expression)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> Cache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var e = Cache.GetOrAdd(propertyName ?? string.Empty, name => new PropertyChangedEventArgs(name));
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace("propertyName ?? string.Empty", expression));
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classDeclaration = syntaxTree.FindClassDeclaration("C");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual("N.C.OnPropertyChanged(string)", OnPropertyChanged.Find(type, semanticModel, CancellationToken.None).ToString());
            }
        }
    }
}
