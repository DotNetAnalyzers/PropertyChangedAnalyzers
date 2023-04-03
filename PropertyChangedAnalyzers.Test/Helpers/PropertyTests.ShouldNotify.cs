namespace PropertyChangedAnalyzers.Test.Helpers;

using System.Threading;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

public static partial class PropertyTests
{
    public static class ShouldNotify
    {
        [TestCase("P1", false)]
        [TestCase("P2", true)]
        [TestCase("P3", true)]
        [TestCase("P4", false)]
        [TestCase("P5", true)]
        [TestCase("P6", true)]
        [TestCase("P7", false)]
        [TestCase("P8", true)]
        [TestCase("P9", true)]
        [TestCase("P10", true)]
        public static void MiscProperties(string propertyName, bool expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
    public class C
    {
        public C(int p4, int p5)
        {
            this.P4 = value4;
            this.P5 = value5;
            this.P10 = 1;
        }

        public int P1 { get; }

        public int P2 { get; set; }

        public int P3 { get; protected set; }

        public int P4 { get; private set; }

        public int P5 { get; private set; }

        public int P6 { get; private set; }

        internal int P7 { get; }

        internal int P8 { get; set; }

        internal int P9 { get; private set; }

        internal int P10 { get; private set; }

        public void Mutate()
        {
            this.P5++;
            this.P6--;
            this.P10 = 2;
        }
    }");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var propertyDeclaration = syntaxTree.FindPropertyDeclaration(propertyName);
            var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
            Assert.AreEqual(expected, Property.ShouldNotify(propertyDeclaration, propertySymbol, semanticModel, CancellationToken.None));
        }

        [TestCase("P1", false)]
        [TestCase("P2", true)]
        [TestCase("P3", true)]
        [TestCase("P4", false)]
        [TestCase("P5", true)]
        [TestCase("P6", true)]
        [TestCase("P7", false)]
        [TestCase("P8", true)]
        [TestCase("P9", true)]
        [TestCase("P10", true)]
        public static void MiscPropertiesUnderscoreNames(string propertyName, bool expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    public class C
    {
        public C(int p4, int p5)
        {
            P4 = value4;
            P5 = value5;
            P10 = 1;
        }

        public int P1 { get; }

        public int P2 { get; set; }

        public int P3 { get; protected set; }

        public int P4 { get; private set; }

        public int P5 { get; private set; }

        public int P6 { get; private set; }

        internal int P7 { get; }

        internal int P8 { get; set; }

        internal int P9 { get; private set; }

        internal int P10 { get; private set; }

        public void Mutate()
        {
            P5++;
            P6--;
            P10 = 1;
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var propertyDeclaration = syntaxTree.FindPropertyDeclaration(propertyName);
            var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
            Assert.AreEqual(expected, Property.ShouldNotify(propertyDeclaration, propertySymbol, semanticModel, CancellationToken.None));
        }

        [TestCase("this.P = 1;")]
        [TestCase("this.P++")]
        [TestCase("this.P--")]
        public static void PrivateSetAssignedInLambdaInCtor(string assignCode)
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public C()
        {
            E += (_, __) => this.P = 1;
        }

        public event EventHandler E;

        public int P { get; private set; }
    }
}".AssertReplace("this.P = 1", assignCode);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var propertyDeclaration = syntaxTree.FindPropertyDeclaration("P");
            var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
            Assert.AreEqual(true, Property.ShouldNotify(propertyDeclaration, propertySymbol, semanticModel, CancellationToken.None));
        }
    }
}
