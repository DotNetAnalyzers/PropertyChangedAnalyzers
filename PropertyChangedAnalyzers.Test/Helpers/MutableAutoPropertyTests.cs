namespace PropertyChangedAnalyzers.Test.Helpers
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static class MutableAutoPropertyTests
    {
        [TestCase("P1", false)]
        [TestCase("P2", true)]
        [TestCase("P3", false)]
        [TestCase("P4", false)]
        [TestCase("P5", false)]
        [TestCase("P6", false)]
        public static void Match(string propertyName, bool expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C
    {
        private readonly int p3;
        private readonly int p4;
        private int p5;
        private int p6;

        public int P1 { get; }

        public int P2 { get; set; }

        public int P3 => this.p3;

        public int P4
        {
            get
            {
                return this.p4;
            }
        }

        public int P5
        {
            get { return this.p5; }
            set { this.p5 = value; }
        }

        public int P6
        {
            get => this.p6;
            private set => this.p6 = value;
        }
    }
}");
            var property = syntaxTree.FindPropertyDeclaration(propertyName);
            Assert.AreEqual(expected, MutableAutoProperty.Match(property) is { });
        }
    }
}
