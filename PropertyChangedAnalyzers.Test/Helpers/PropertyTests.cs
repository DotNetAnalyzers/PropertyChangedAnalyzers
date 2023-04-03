namespace PropertyChangedAnalyzers.Test.Helpers;

using System.Threading;

using Gu.Roslyn.Asserts;

using Microsoft.CodeAnalysis.CSharp;

using NUnit.Framework;

public partial class PropertyTests
{
    [TestCase("P1", false)]
    [TestCase("P2", false)]
    [TestCase("P3", false)]
    [TestCase("P4", false)]
    [TestCase("Lazy1", true)]
    [TestCase("Lazy2", true)]
    [TestCase("Lazy3", true)]
    [TestCase("Lazy4", true)]
    [TestCase("Lazy5", true)]
    public static void IsLazy(string code, bool expected)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    using System;

    public class C
    {
        private readonly int p;
        private int p5;

        private string lazy1;
        private string lazy2;
        private string lazy3;
        private string lazy4;
        private Action lazy5;
 
        public int P1 { get; }

        public int P2 => this.p;
       
        public string Lazy1
        {
            get
            {
                if (this.lazy1 != null)
                {
                    return this.lazy1;
                }

                this.lazy1 = new string(' ', 1);
                return this.lazy1;
            }
        }

        public string Lazy2
        {
            get
            {
                if (this.lazy2 == null)
                {
                    this.lazy2 = new string(' ', 1);
                }

                return this.lazy2;
            }
        }

        public string Lazy3
        {
            get
            {
                return this.lazy3 ?? (this.lazy3 = new string(' ', 1));
            }
        }

        public string Lazy4 => this.lazy4 ?? (this.lazy4 = new string(' ', 1));

        public Action Lazy5 => this.lazy5 ?? (this.lazy5 = new Action(() => this.lazy5 = null));

        public int P3
        {
            get
            {
                this.p;
            }
        }

        public int P4 { get; set; }

        public int P5
        {
            get { return this.p5; }
            set { this.p5 = value; }
        }
    }
}");
        var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var property = syntaxTree.FindPropertyDeclaration(code);
        Assert.AreEqual(expected, Property.IsLazy(property, semanticModel, CancellationToken.None));
    }
}
