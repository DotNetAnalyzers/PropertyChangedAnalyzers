namespace PropertyChangedAnalyzers.Test
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    internal partial class PropertyChangedTest
    {
        internal class IsInvoker
        {
            [TestCase("protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)")]
            [TestCase("protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)")]
            [TestCase("protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)")]
            public void WhenTrue(string singature)
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
                var compilation = CSharpCompilation.Create(
                    "test",
                    new[] { syntaxTree },
                    MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var methodDeclaration = syntaxTree.FindMethodDeclaration(singature);
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.IsInvoker(method, semanticModel, CancellationToken.None));
            }
        }
    }
}