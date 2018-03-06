namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class EqualityTests
    {
        [TestCase("Nullable.Equals(this.bar1, this.bar1)", true)]
        [TestCase("Nullable.Equals(bar1, this.bar1)", true)]
        [TestCase("Nullable.Equals(bar1, bar1)", true)]
        [TestCase("Nullable.Equals(this.bar1, this.Bar1)", true)]
        [TestCase("Nullable.Equals(this.bar1, this.bar3)", true)]
        [TestCase("Nullable.Equals(this.bar3, this.bar1)", true)]
        [TestCase("System.Nullable.Equals(this.bar1, this.bar1)", true)]
        [TestCase("System.Nullable.Equals(bar1, this.bar1)", true)]
        [TestCase("System.Nullable.Equals(this.bar1, this.Bar1)", true)]
        [TestCase("Nullable.Equals(this.bar1, this.bar1)", true)]
        [TestCase("System.Nullable.Equals(this.bar1, this.bar2)", true)]
        [TestCase("Nullable.Equals(this.bar1, this.bar4)", false)]
        [TestCase("System.Nullable.Equals(this.bar, this.bar4)", false)]
        public void IsNullableEquals(string check, bool expected)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        private int? bar1;
        private Nullable<int> bar2;
        private int bar3;
        private double? bar4;
        private string bar5;

        public Foo()
        {
            Nullable.Equals(this.bar1, this.bar1);
        }

        public int? Bar1 => this.bar1;
    }
}";

            testCode = testCode.AssertReplace("Nullable.Equals(this.bar1, this.bar1)", check);
            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation(check);
            var arg0 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, CancellationToken.None);
            var arg1 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[1].Expression, CancellationToken.None);
            Assert.AreEqual(expected, Equality.IsNullableEquals(invocation, semanticModel, CancellationToken.None, arg0, arg1));
        }
    }
}
