namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    internal partial class PropertyTests
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
            var property = syntaxTree.FindPropertyDeclaration(code);
            Assert.AreEqual(expected, Property.IsLazy(property, semanticModel, CancellationToken.None));
        }

        [TestCase("Value1", false, null)]
        [TestCase("Value2", false, null)]
        [TestCase("Value3", false, null)]
        [TestCase("Value4", false, null)]
        [TestCase("Value5", true, "value5")]
        [TestCase("Value6", true, "value6")]
        public void TryGetBackingField(string propertyName, bool expected, string fieldName)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandBox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

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
            private set => this.value6 = value;
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var property = syntaxTree.FindPropertyDeclaration(propertyName);
            Assert.AreEqual(expected, Property.TryGetBackingFieldFromSetter(property, semanticModel, CancellationToken.None, out var field));
            Assert.AreEqual(fieldName, field?.Name);
        }

        [TestCase("Value1", false)]
        [TestCase("Value2", true)]
        [TestCase("Value3", false)]
        [TestCase("Value4", false)]
        [TestCase("Value5", false)]
        [TestCase("Value6", false)]
        public void IsMutableAutoProperty(string propertyName, bool expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandBox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

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
            private set => this.value6 = value;
        }
    }
}");
            var property = syntaxTree.FindPropertyDeclaration(propertyName);
            Assert.AreEqual(expected, Property.IsMutableAutoProperty(property));
        }

        [TestCase("Value11", "this.value1")]
        [TestCase("Value12", "this.value1")]
        [TestCase("Value1", "this.value1")]
        [TestCase("Value2", "this.value2")]
        public void TryGetSingleReturnedInGetter(string propertyName, string expected)
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
            Assert.AreEqual(true, Property.TryGetSingleReturnedInGetter(declaration, out var expression));
            Assert.AreEqual(expected, expression.ToString());
        }
    }
}
