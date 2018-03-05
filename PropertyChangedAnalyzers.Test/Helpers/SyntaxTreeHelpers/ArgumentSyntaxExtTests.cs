namespace PropertyChangedAnalyzers.Test.Helpers.SyntaxTreeHelpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    public class ArgumentSyntaxExtTests
    {
        [TestCase("\"text\"", "text")]
        [TestCase("string.Empty", "")]
        [TestCase("String.Empty", "")]
        public void TryGetStringValue(string code, string expected)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            Bar(""text"");
            Bar(string.Empty);
            Bar(String.Empty);
        }

        private void Bar(string arg)
        {
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindBestMatch<ArgumentSyntax>(code);
            Assert.AreEqual(true, invocation.TryGetStringValue(semanticModel, CancellationToken.None, out var name));
            Assert.AreEqual(expected, name);
        }
    }
}
