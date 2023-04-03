namespace PropertyChangedAnalyzers.Test.Helpers;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

public static class GetterTests
{
    [TestCase("P11", "this.p1")]
    [TestCase("P12", "this.p1")]
    [TestCase("P1", "this.p1")]
    [TestCase("P2", "this.p2")]
    public static void TrySingleReturned(string propertyName, string expected)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            @"
namespace N
{
    public class C
    {
        private int p1;
        private int p2;

        public int P11 => this.p1;

        public int P12
        {
            get => this.p1;
        }

        public int P1
        {
            get { return this.p1; }
            set { this.p1 = value; }
        }

        public int P2
        {
            get => this.p2;
            set => this.p2 = value;
        }
    }
}");
        var declaration = syntaxTree.FindPropertyDeclaration(propertyName);
        Assert.AreEqual(expected, Property.FindSingleReturned(declaration).ToString());
    }
}
