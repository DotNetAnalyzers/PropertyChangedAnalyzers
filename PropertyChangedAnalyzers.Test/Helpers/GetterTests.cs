namespace PropertyChangedAnalyzers.Test.Helpers
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static class GetterTests
    {
        [TestCase("Value11", "this.value1")]
        [TestCase("Value12", "this.value1")]
        [TestCase("Value1", "this.value1")]
        [TestCase("Value2", "this.value2")]
        public static void TrySingleReturned(string propertyName, string expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int value1;
        private int value2;

        public int Value11 => this.value1;

        public int Value12
        {
            get => this.value1;
        }

        public int Value1
        {
            get { return this.value1; }
            set { this.value1 = value; }
        }

        public int Value2
        {
            get => this.value2;
            set => this.value2 = value;
        }
    }
}");
            var declaration = syntaxTree.FindPropertyDeclaration(propertyName);
            Assert.AreEqual(true, Property.TrySingleReturned(declaration, out var expression));
            Assert.AreEqual(expected, expression.ToString());
        }
    }
}
