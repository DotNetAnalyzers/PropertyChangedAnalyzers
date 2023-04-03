namespace PropertyChangedAnalyzers.Test.Helpers.SyntaxTreeHelpers;

using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

public class PropertyDeclarationSyntaxExtTests
{
    [TestCase("P1", "get { return this.p1; }")]
    [TestCase("P2", "private get { return value2; }")]
    public void TryGetGetAccessorDeclarationBlock(string propertyName, string getter)
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
            private get { return value2; }
            set { value2 = value; }
        }
    }
}");
        var property = syntaxTree.FindPropertyDeclaration(propertyName);
        Assert.AreEqual(true, property.TryGetGetter(out var result));
        Assert.AreEqual(getter, result.ToString());
    }

    [TestCase("P1", "set { this.p1 = value; }")]
    [TestCase("P2", "private set { value2 = value; }")]
    public void TryGetSetAccessorDeclarationBlock(string propertyName, string setter)
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
