namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static partial class PropertyTests
    {
        public static class ShouldNotify
        {
            [TestCase("Value1", false)]
            [TestCase("Value2", true)]
            [TestCase("Value3", true)]
            [TestCase("Value4", false)]
            [TestCase("Value5", true)]
            [TestCase("Value6", true)]
            [TestCase("Value7", false)]
            [TestCase("Value8", true)]
            [TestCase("Value9", true)]
            [TestCase("Value10", true)]
            public static void MiscProperties(string propertyName, bool expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
    public class Foo
    {
        public Foo(int value4, int value5)
        {
            this.Value4 = value4;
            this.Value5 = value5;
            this.Value10 = 1;
        }

        public int Value1 { get; }

        public int Value2 { get; set; }

        public int Value3 { get; protected set; }

        public int Value4 { get; private set; }

        public int Value5 { get; private set; }

        public int Value6 { get; private set; }

        internal int Value7 { get; }

        internal int Value8 { get; set; }

        internal int Value9 { get; private set; }

        internal int Value10 { get; private set; }

        public void Mutate()
        {
            this.Value5++;
            this.Value6--;
            this.Value10 = 2;
        }
    }");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var propertyDeclaration = syntaxTree.FindPropertyDeclaration(propertyName);
                var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
                Assert.AreEqual(expected, Property.ShouldNotify(propertyDeclaration, propertySymbol, semanticModel, CancellationToken.None));
            }

            [TestCase("Value1", false)]
            [TestCase("Value2", true)]
            [TestCase("Value3", true)]
            [TestCase("Value4", false)]
            [TestCase("Value5", true)]
            [TestCase("Value6", true)]
            [TestCase("Value7", false)]
            [TestCase("Value8", true)]
            [TestCase("Value9", true)]
            [TestCase("Value10", true)]
            public static void MiscPropertiesUnderscoreNames(string propertyName, bool expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo(int value4, int value5)
        {
            Value4 = value4;
            Value5 = value5;
            Value10 = 1;
        }

        public int Value1 { get; }

        public int Value2 { get; set; }

        public int Value3 { get; protected set; }

        public int Value4 { get; private set; }

        public int Value5 { get; private set; }

        public int Value6 { get; private set; }

        internal int Value7 { get; }

        internal int Value8 { get; set; }

        internal int Value9 { get; private set; }

        internal int Value10 { get; private set; }

        public void Mutate()
        {
            Value5++;
            Value6--;
            Value10 = 1;
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var propertyDeclaration = syntaxTree.FindPropertyDeclaration(propertyName);
                var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
                Assert.AreEqual(expected, Property.ShouldNotify(propertyDeclaration, propertySymbol, semanticModel, CancellationToken.None));
            }

            [TestCase("this.Value = 1;")]
            [TestCase("this.Value++")]
            [TestCase("this.Value--")]
            public static void PrivateSetAssignedInLambdaInCtor(string assignCode)
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            Bar += (_, __) => this.Value = 1;
        }

        public event EventHandler Bar;

        public int Value { get; private set; }
    }
}".AssertReplace("this.Value = 1", assignCode);

                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var propertyDeclaration = syntaxTree.FindPropertyDeclaration("Value");
                var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
                Assert.AreEqual(true, Property.ShouldNotify(propertyDeclaration, propertySymbol, semanticModel, CancellationToken.None));
            }
        }
    }
}
