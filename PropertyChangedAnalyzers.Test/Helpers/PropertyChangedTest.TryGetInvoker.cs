namespace PropertyChangedAnalyzers.Test
{
    using System.Linq;
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    internal partial class PropertyChangedTest
    {
        internal class TryGetInvoker
        {
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

        ↓public int Bar { get; set; }

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
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] {syntaxTree},
                    MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classDeclaration = syntaxTree.FindBestMatch<ClassDeclarationSyntax>("Foo");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(
                    true,
                    PropertyChanged.TryGetInvoker(type, semanticModel, CancellationToken.None, out var invoker));
                Assert.AreEqual("OnPropertyChanged", invoker.Name);
                Assert.AreEqual(
                    "String",
                    invoker.Parameters.Single()
                           .Type.MetadataName);
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

        ↓public int Bar { get; set; }

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
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] {syntaxTree},
                    MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classDeclaration = syntaxTree.FindBestMatch<ClassDeclarationSyntax>("Foo");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(
                    true,
                    PropertyChanged.TryGetInvoker(type, semanticModel, CancellationToken.None, out var invoker));
                Assert.AreEqual("OnPropertyChanged", invoker.Name);
                Assert.AreEqual(
                    "String",
                    invoker.Parameters.Single()
                           .Type.MetadataName);
            }
        }
    }
}