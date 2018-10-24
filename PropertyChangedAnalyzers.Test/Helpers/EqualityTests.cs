namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    public class EqualityTests
    {
        [TestCase("this.bar1 == this.bar1", true)]
        [TestCase("this.bar1 == bar1", true)]
        [TestCase("this.bar1 == bar2", true)]
        [TestCase("this.bar1 != bar2", false)]
        [TestCase("this.bar1 == missing", false)]
        public void IsOperatorEquals(string check, bool expected)
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
            this.bar1 == this.bar1;
        }

        public int? Bar1 => this.bar1;
    }
}".AssertReplace("this.bar1 == this.bar1", check);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var binary = syntaxTree.FindBinaryExpression(check);
            var arg0 = semanticModel.GetSymbolSafe(binary.Left, CancellationToken.None);
            var arg1 = semanticModel.GetSymbolSafe(binary.Right, CancellationToken.None);
            Assert.AreEqual(expected, Equality.IsOperatorEquals(binary, semanticModel, CancellationToken.None, arg0, arg1));
            Assert.AreEqual(expected, Equality.IsOperatorEquals(binary, semanticModel, CancellationToken.None, arg1, arg0));
        }

        [TestCase("this.bar1 != this.bar1", true)]
        [TestCase("this.bar1 != bar1", true)]
        [TestCase("this.bar1 != bar2", true)]
        [TestCase("this.bar1 == bar2", false)]
        [TestCase("this.bar1 != missing", false)]
        public void IsOperatorNotEquals(string check, bool expected)
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
            this.bar1 == this.bar1;
        }

        public int? Bar1 => this.bar1;
    }
}".AssertReplace("this.bar1 == this.bar1", check);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var binary = syntaxTree.FindBinaryExpression(check);
            var arg0 = semanticModel.GetSymbolSafe(binary.Left, CancellationToken.None);
            var arg1 = semanticModel.GetSymbolSafe(binary.Right, CancellationToken.None);
            Assert.AreEqual(expected, Equality.IsOperatorNotEquals(binary, semanticModel, CancellationToken.None, arg0, arg1));
            Assert.AreEqual(expected, Equality.IsOperatorNotEquals(binary, semanticModel, CancellationToken.None, arg1, arg0));
        }

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
        [TestCase("System.Nullable.Equals(this.bar1, this.bar4)", false)]
        [TestCase("System.Nullable.Equals(this.bar1, this.bar5)", false)]
        [TestCase("System.Nullable.Equals(this.bar1, missing)", false)]
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
}".AssertReplace("Nullable.Equals(this.bar1, this.bar1)", check);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation(check);
            var arg0 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, CancellationToken.None);
            var arg1 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[1].Expression, CancellationToken.None);
            Assert.AreEqual(expected, Equality.IsNullableEquals(invocation, semanticModel, CancellationToken.None, arg0, arg1));
            Assert.AreEqual(expected, Equality.IsNullableEquals(invocation, semanticModel, CancellationToken.None, arg1, arg0));
        }

        [TestCase("Equals(this.bar1, this.bar1)", true)]
        [TestCase("object.Equals(this.bar1, this.bar1)", true)]
        [TestCase("Object.Equals(this.bar1, this.bar1)", true)]
        [TestCase("System.Object.Equals(this.bar1, this.bar1)", true)]
        [TestCase("Equals(this.bar1, missing)", false)]
        [TestCase("object.Equals(this.bar1, missing)", false)]
        [TestCase("Object.Equals(this.bar1, missing)", false)]
        [TestCase("Nullable.Equals(bar1, this.bar1)", false)]
        [TestCase("Nullable.Equals(bar1, bar1)", false)]
        [TestCase("Nullable.Equals(this.bar1, this.Bar1)", false)]
        [TestCase("Nullable.Equals(this.bar1, this.bar3)", false)]
        [TestCase("System.Nullable.Equals(this.bar1, this.bar1)", false)]
        [TestCase("System.Nullable.Equals(bar1, this.bar1)", false)]
        [TestCase("System.Nullable.Equals(this.bar1, this.Bar1)", false)]
        [TestCase("Nullable.Equals(this.bar1, this.bar1)", false)]
        [TestCase("System.Nullable.Equals(this.bar1, this.bar2)", false)]
        [TestCase("Nullable.Equals(this.bar1, this.bar4)", false)]
        [TestCase("System.Nullable.Equals(this.bar1, this.bar4)", false)]
        [TestCase("System.Nullable.Equals(this.bar1, this.bar5)", true)]
        [TestCase("System.Nullable.Equals(this.bar1, missing)", false)]
        public void IsObjectEquals(string check, bool expected)
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
            Equals(this.bar1, this.bar1);
        }

        public int? Bar1 => this.bar1;
    }
}".AssertReplace("Equals(this.bar1, this.bar1)", check);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation(check);
            var arg0 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, CancellationToken.None);
            var arg1 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[1].Expression, CancellationToken.None);
            Assert.AreEqual(expected, Equality.IsObjectEquals(invocation, semanticModel, CancellationToken.None, arg0, arg1));
            Assert.AreEqual(expected, Equality.IsObjectEquals(invocation, semanticModel, CancellationToken.None, arg1, arg0));
        }

        [TestCase("ReferenceEquals(this.bar1, this.bar1)", true)]
        [TestCase("ReferenceEquals(this.bar1, this.Bar1)", true)]
        [TestCase("object.ReferenceEquals(this.bar1, this.bar1)", true)]
        [TestCase("Object.ReferenceEquals(this.bar1, this.bar1)", true)]
        [TestCase("System.Object.ReferenceEquals(this.bar1, this.bar1)", true)]
        [TestCase("ReferenceEquals(this.bar1, missing)", false)]
        [TestCase("object.ReferenceEquals(this.bar1, missing)", false)]
        [TestCase("Object.ReferenceEquals(this.bar1, missing)", false)]
        public void IsReferenceEquals(string check, bool expected)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        private string bar1;
        private string bar2;

        public Foo()
        {
            ReferenceEquals(this.bar1, this.bar1);
        }

        public string Bar1 => this.bar1;
    }
}".AssertReplace("ReferenceEquals(this.bar1, this.bar1)", check);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation(check);
            var arg0 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, CancellationToken.None);
            var arg1 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[1].Expression, CancellationToken.None);
            Assert.AreEqual(expected, Equality.IsReferenceEquals(invocation, semanticModel, CancellationToken.None, arg0, arg1));
            Assert.AreEqual(expected, Equality.IsReferenceEquals(invocation, semanticModel, CancellationToken.None, arg1, arg0));
        }

        [TestCase("string.Equals(this.bar1, this.bar1)", true)]
        [TestCase("String.Equals(this.bar1, this.bar1)", true)]
        [TestCase("System.String.Equals(this.bar1, this.bar1)", true)]
        [TestCase("string.Equals(this.bar1, this.bar1, StringComparison.OrdinalIgnoreCase)", true)]
        [TestCase("String.Equals(this.bar1, this.bar1, StringComparison.OrdinalIgnoreCase)", true)]
        [TestCase("System.String.Equals(this.bar1, this.bar1, StringComparison.OrdinalIgnoreCase)", true)]
        [TestCase("string.Equals(this.bar1, missing)", false)]
        [TestCase("string.Equals(this.bar1, this.bar3)", false)]
        public void IsStringEquals(string check, bool expected)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        private string bar1;
        private string bar2;
        private int bar3;

        public Foo()
        {
            string.Equals(this.bar1, this.bar1);
        }
    }
}".AssertReplace("string.Equals(this.bar1, this.bar1)", check);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation(check);
            var arg0 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, CancellationToken.None);
            var arg1 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[1].Expression, CancellationToken.None);
            Assert.AreEqual(expected, Equality.IsStringEquals(invocation, semanticModel, CancellationToken.None, arg0, arg1));
            Assert.AreEqual(expected, Equality.IsStringEquals(invocation, semanticModel, CancellationToken.None, arg1, arg0));
        }

        [TestCase("this.bar1.Equals(this.bar1)", true)]
        [TestCase("bar1.Equals(bar1)", true)]
        [TestCase("bar1.Equals(bar2)", true)]
        [TestCase("this.bar1.Equals(missing)", false)]
        [TestCase("missing.Equals(this.bar1)", false)]
        public void IsInstanceEquals(string check, bool expected)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        private string bar1;
        private string bar2;
        private int bar3;

        public Foo()
        {
            this.bar1.Equals(this.bar1);
        }
    }
}".AssertReplace("this.bar1.Equals(this.bar1)", check);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation(check);
            var instance = semanticModel.GetSymbolSafe(((MemberAccessExpressionSyntax)invocation.Expression).Expression, CancellationToken.None);
            var arg = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, CancellationToken.None);
            Assert.AreEqual(expected, Equality.IsInstanceEquals(invocation, semanticModel, CancellationToken.None, instance, arg));
            Assert.AreEqual(ReferenceEquals(instance, arg), Equality.IsInstanceEquals(invocation, semanticModel, CancellationToken.None, arg, instance));
        }

        [TestCase("EqualityComparer<int>.Default.Equals(this.bar1, this.bar1)", true)]
        [TestCase("System.Collections.Generic.EqualityComparer<int>.Default.Equals(this.bar1, this.bar1)", true)]
        [TestCase("EqualityComparer.Equals(this.bar1, this.bar1)", true)]
        [TestCase("EqualityComparer<int>.Default.Equals(missing, this.bar3)", false)]
        [TestCase("EqualityComparer.Equals(missing, this.bar3)", false)]
        [TestCase("EqualityComparer<int>.Default.Equals(missing, this.bar1)", false)]
        [TestCase("EqualityComparer<int>.Default.Equals(this.bar1, missing)", false)]
        public void IsEqualityComparerEquals(string check, bool expected)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        private static readonly EqualityComparer<int> EqualityComparer = EqualityComparer<int>.Default;
        private int bar1;
        private int bar2;
        private string bar3;

        public Foo()
        {
            EqualityComparer<int>.Default.Equals(this.bar1, this.bar1);
        }

        public int? Bar1 => this.bar1;
    }
}".AssertReplace("EqualityComparer<int>.Default.Equals(this.bar1, this.bar1)", check);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation(check);
            var arg0 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, CancellationToken.None);
            var arg1 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[1].Expression, CancellationToken.None);
            Assert.AreEqual(expected, Equality.IsEqualityComparerEquals(invocation, semanticModel, CancellationToken.None, arg0, arg1));
            Assert.AreEqual(expected, Equality.IsEqualityComparerEquals(invocation, semanticModel, CancellationToken.None, arg1, arg0));
        }

        [TestCase("EqualityComparer<T>.Default.Equals(arg1, arg1)", true)]
        public void IsEqualityComparerEqualsGeneric(string check, bool expected)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;

    public class Foo
    {
        private int bar1;
        private int bar2;
        private string bar5;

        public bool Bar<T>(T arg1, T arg2)
        {
            return EqualityComparer<T>.Default.Equals(arg1, arg1);
        }

        public int? Bar1 => this.bar1;
    }
}".AssertReplace("EqualityComparer<T>.Default.Equals(arg1, arg1)", check);

            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var invocation = syntaxTree.FindInvocation(check);
            var arg0 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, CancellationToken.None);
            var arg1 = semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[1].Expression, CancellationToken.None);
            Assert.AreEqual(expected, Equality.IsEqualityComparerEquals(invocation, semanticModel, CancellationToken.None, arg0, arg1));
            Assert.AreEqual(expected, Equality.IsEqualityComparerEquals(invocation, semanticModel, CancellationToken.None, arg1, arg0));
        }
    }
}
