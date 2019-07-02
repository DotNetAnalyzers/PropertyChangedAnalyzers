namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public partial class PropertyChangedTest
    {
        public static class TryGetBackingFieldAssignedInSetter
        {
            [Test]
            public static void Simple()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int bar;

        public int Bar
        {
            get { return this.bar; }
            set { this.bar = value; }
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classDeclaration = syntaxTree.FindPropertyDeclaration("Bar");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(true, Property.TryGetBackingFieldFromSetter(type, semanticModel, CancellationToken.None, out var field));
                Assert.AreEqual("bar", field.Name);
                Assert.AreEqual("Int32", field.Type.MetadataName);
            }

            [Test]
            public static void SetAndRaise()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
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
            get { return this.bar; }
            set { this.TrySet(ref bar, value); }
        }

        protected bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
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
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classDeclaration = syntaxTree.FindPropertyDeclaration("Bar");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(true, Property.TryGetBackingFieldFromSetter(type, semanticModel, CancellationToken.None, out var field));
                Assert.AreEqual("bar", field.Name);
                Assert.AreEqual("Int32", field.Type.MetadataName);
            }

            [Test]
            public static void RecursiveSetAndRaise()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
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
            get { return this.bar; }
            set { this.TrySet(ref bar, value); }
        }

        protected bool TrySet<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (this.TrySet(ref field, newValue, propertyName))
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
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classDeclaration = syntaxTree.FindPropertyDeclaration("Bar");
                var type = semanticModel.GetDeclaredSymbol(classDeclaration);
                Assert.AreEqual(false, Property.TryGetBackingFieldFromSetter(type, semanticModel, CancellationToken.None, out _));
            }
        }
    }
}
