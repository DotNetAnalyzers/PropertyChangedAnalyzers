namespace PropertyChangedAnalyzers.Test
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class PropertyTests
    {
        [TestCase("Value1", false)]
        [TestCase("Value2", false)]
        [TestCase("Value3", false)]
        [TestCase("Value4", false)]
        [TestCase("Lazy1", true)]
        [TestCase("Lazy2", true)]
        [TestCase("Lazy3", true)]
        [TestCase("Lazy4", true)]
        [TestCase("Lazy5", true)]
        public void IsLazy(string code, bool expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandBox
{
    using System;

    public class Foo
    {
        private readonly int value;
        private int value5;

        private string lazy1;
        private string lazy2;
        private string lazy3;
        private string lazy4;
        private Action lazy5;
 
        public int Value1 { get; }

        public int Value2 => this.value;
       
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

        public int Value3
        {
            get
            {
                this.value;
            }
        }

        public int Value4 { get; set; }

        public int Value5
        {
            get { return this.value5; }
            set { this.value5 = value; }
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var property = syntaxTree.PropertyDeclarationSyntax(code);
            Assert.AreEqual(expected, Property.IsLazy(property, semanticModel, CancellationToken.None));
        }

        [TestCase("Value1", false, null)]
        [TestCase("Value2", false, null)]
        [TestCase("Value3", false, null)]
        [TestCase("Value4", false, null)]
        [TestCase("Value5", true, "value5")]
        [TestCase("Value6", true, "value6")]
        public void TryGetBackingField(string propertyName, bool expected, string field)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandBox
{
    using System;

    public class Foo
    {
        private readonly int value3;
        private readonly int value4;
        private int value5;
        private int value6;

        public int Value1 { get; }

        public int Value2 { get; set; }

        public int Value3 => this.value3;

        public int Value4
        {
            get
            {
                return this.value4;
            }
        }

        public int Value5
        {
            get { return this.value5; }
            set { this.value5 = value; }
        }

        public int Value6
        {
            get => this.value6;
            set => this.value6 = value;
        }
    }
}");
            var property = syntaxTree.PropertyDeclarationSyntax(propertyName);
            Assert.AreEqual(expected, Property.TryGetBackingField(property, out var identifier, out var declaration));
            Assert.AreEqual(field, identifier?.Identifier.Text);
            Assert.AreEqual(field, declaration?.Name());
        }

        [Test]
        public void TryGetBackingFieldExpressionBodyAccessor()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandBox
{
    using System;

    public class Foo
    {
        private int value;

        public int Value
        {
            get => this.value;
            set => this.value = value;
        }
    }
}");
            var property = syntaxTree.PropertyDeclarationSyntax("Value");
            Assert.AreEqual(true, Property.TryGetBackingField(property, out var identifier, out var declaration));
            Assert.AreEqual("value", identifier?.Identifier.Text);
            Assert.AreEqual("value", declaration?.Name());
        }
    }
}
