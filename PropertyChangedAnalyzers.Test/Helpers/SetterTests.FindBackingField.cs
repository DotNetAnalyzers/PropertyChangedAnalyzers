namespace PropertyChangedAnalyzers.Test.Helpers;

using System.Threading;
using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

public static class SetterTests
{
    public static class FindBackingField
    {
        [TestCase("P1", "p1")]
        [TestCase("P2", "p2")]
        public static void FindBackingFieldWhenTwo(string propertyName, string fieldName)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    public class C
    {
        private int p1;
        private int p2;

        public int P1
        {
            get { return this.p1; }
            set { this.p1 = value; }
        }

        public int P2
        {
            get => this.p2;
            private set => this.p2 = value;
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var property = syntaxTree.FindPropertyDeclaration(propertyName);
            Assert.AreEqual(true, property.TryGetSetter(out var setter));
            Assert.AreEqual(fieldName, Setter.FindBackingField(setter, semanticModel, CancellationToken.None)?.Name);
        }

        [Test]
        public static void FindBackingFieldSimple()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                @"
namespace N
{
    public class C
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set { this.p = value; }
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var property = syntaxTree.FindPropertyDeclaration("P");
            Assert.AreEqual(true, property.TryGetSetter(out var setter));
            Assert.AreEqual("p", Setter.FindBackingField(setter, semanticModel, CancellationToken.None).Name);
        }

        [Test]
        public static void FindBackingFieldTrySet()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get { return this.p; }
            set { this.TrySet(ref p, value); }
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var property = syntaxTree.FindPropertyDeclaration("P");
            Assert.AreEqual(true, property.TryGetSetter(out var setter));
            Assert.AreEqual("p", Setter.FindBackingField(setter, semanticModel, CancellationToken.None).Name);
        }

        [Test]
        public static void RecursiveTrySet()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get { return this.p; }
            set { this.TrySet(ref p, value); }
        }

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (this.TrySet(ref field, value, propertyName))
            {
                this.OnPropertyChanged(propertyName);
            }

            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var property = syntaxTree.FindPropertyDeclaration("P");
            Assert.AreEqual(true, property.TryGetSetter(out var setter));
            Assert.AreEqual(null, Setter.FindBackingField(setter, semanticModel, CancellationToken.None));
        }
    }
}
