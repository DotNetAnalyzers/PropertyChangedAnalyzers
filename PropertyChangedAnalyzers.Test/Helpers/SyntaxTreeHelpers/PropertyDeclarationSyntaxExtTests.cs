namespace PropertyChangedAnalyzers.Test.Helpers.SyntaxTreeHelpers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class PropertyDeclarationSyntaxExtTests
    {
        [TestCase("Value1", "get { return this.value1; }")]
        [TestCase("Value2", "private get { return value2; }")]
        public void TryGetGetAccessorDeclarationBlock(string propertyName, string getter)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    public class C
    {
        private int value1;
        private int value2;

        public int Value1
        {
            get { return this.value1; }
            set { this.value1 = value; }
        }

        public int Value2
        {
            private get { return value2; }
            set { value2 = value; }
        }
    }
}");
            var property = syntaxTree.FindPropertyDeclaration(propertyName);
            Assert.AreEqual(true, property.TryGetGetter(out var result));
            Assert.AreEqual(getter, result.ToString());
        }

        [TestCase("Value1", "set { this.value1 = value; }")]
        [TestCase("Value2", "private set { value2 = value; }")]
        public void TryGetSetAccessorDeclarationBlock(string propertyName, string setter)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    public class C
    {
        private int value1;
        private int value2;

        public int Value1
        {
            get { return this.value1; }
            set { this.value1 = value; }
        }

        public int Value2
        {
            get { return value2; }
            private set { value2 = value; }
        }
    }
}");
            var property = syntaxTree.FindPropertyDeclaration(propertyName);
            Assert.AreEqual(true, property.TryGetSetter(out var result));
            Assert.AreEqual(setter, result.ToString());
        }
    }
}
