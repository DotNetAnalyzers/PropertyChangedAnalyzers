namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class MemberPathTests
    {
        internal new class Equals
        {
            [TestCase("get => this.value1;", "set => this.value1 = value;", true)]
            [TestCase("get => this.value1;", "set => value1 = value;", true)]
            [TestCase("get => value1;", "set => this.value1 = value;", true)]
            [TestCase("get => value1;", "set => value1 = value;", true)]
            [TestCase("get => this.value1;", "set => this.value2 = value;", false)]
            [TestCase("get => this.value1;", "set => value2 = value;", false)]
            [TestCase("get => value1;", "set => this.value2 = value;", false)]
            [TestCase("get => value1;", "set => value2 = value;", false)]
            public void Simple(string getter, string setter, bool expected)
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int value1;
        private int value2;

        public int Value
        {
            get => this.value1;
            set => this.value1 = value;
        }
    }
}";
                testCode = testCode.AssertReplace("get => this.value1;", getter)
                                   .AssertReplace("set => this.value1 = value;", setter);
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var propertyDeclaration = syntaxTree.FindPropertyDeclaration("Value");
                var getExpression = propertyDeclaration.Getter().ExpressionBody.Expression;
                var setExpression = ((AssignmentExpressionSyntax)propertyDeclaration.Setter().ExpressionBody.Expression).Left;
                Assert.AreEqual(expected, MemberPath.Equals(getExpression, setExpression));
            }

            [TestCase("get => this.bar1.Value1;", "set => this.bar1.Value1 = value;", true)]
            [TestCase("get => this.bar1.Value1;", "set => bar1.Value1 = value;", true)]
            [TestCase("get => bar1.Value1;", "set => this.bar1.Value1 = value;", true)]
            [TestCase("get => bar1.Value1;", "set => bar1.Value1 = value;", true)]
            [TestCase("get => this.bar1?.Value1;", "set => this.bar1.Value1 = value;", true)]
            [TestCase("get => this.bar1.Value1;", "set => bar1?.Value1 = value;", true)]
            [TestCase("get => bar1?.Value1;", "set => this.bar1?.Value1 = value;", true)]
            [TestCase("get => this.bar1.Value1;", "set => this.bar2.Value1 = value;", false)]
            [TestCase("get => this.bar1.Value1;", "set => bar2.Value1 = value;", false)]
            [TestCase("get => bar1.Value1;", "set => this.bar2.Value1 = value;", false)]
            [TestCase("get => bar1.Value1;", "set => bar2.Value1 = value;", false)]
            [TestCase("get => this.bar1.Value1;", "set => this.bar1 = value;", false)]
            [TestCase("get => this.bar1.Value1;", "set => this.bar2 = value;", false)]
            [TestCase("get => this.bar1.Value1;", "set => bar1 = value;", false)]
            [TestCase("get => this.bar1.Value1;", "set => bar2 = value;", false)]
            [TestCase("get => bar1.Value1;", "set => this.bar1 = value;", false)]
            [TestCase("get => bar1.Value1;", "set => this.bar2 = value;", false)]
            public void Nested(string getter, string setter, bool expected)
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int Value1;
        public int Value2;
    }

    public class Foo
    {
        private Bar bar1 = new Bar();
        private Bar bar2 = new Bar();

        public int Value
        {
            get => this.bar1.Value1;
            set => this.bar2.Value1 = value;
        }
    }
}";
                testCode = testCode.AssertReplace("get => this.bar1.Value1;", getter)
                                   .AssertReplace("set => this.bar2.Value1 = value;", setter);
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var propertyDeclaration = syntaxTree.FindPropertyDeclaration("Value");
                var getExpression = propertyDeclaration.Getter().ExpressionBody.Expression;
                var setExpression = ((AssignmentExpressionSyntax)propertyDeclaration.Setter().ExpressionBody.Expression).Left;
                Assert.AreEqual(expected, MemberPath.Equals(getExpression, setExpression));
            }

            [TestCase("get => this.value1;", "set => this.value1 = value;", true)]
            [TestCase("get => this.value1;", "set => this.value2 = value;", false)]
            [TestCase("get => this.bar1.Value1;", "set => this.bar1.Value1 = value;", true)]
            [TestCase("get => this.bar1.Value1;", "set => this.bar2.Value1 = value;", false)]
            [TestCase("get => this.bar1.Value1;", "set => this.bar1.Value2 = value;", false)]
            public void Uses(string getter, string setter, bool expected)
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public int Value1;
        public int Value2;
    }

    public class Foo
    {
        private Bar bar1 = new Bar();
        private Bar bar2 = new Bar();
        private int value1;
        private int value2;

        public int Value
        {
            get => this.value1;
            set => this.value1 = value;
        }
    }
}";
                testCode = testCode.AssertReplace("get => this.value1;", getter)
                                   .AssertReplace("set => this.value1 = value;", setter);
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var propertyDeclaration = syntaxTree.FindPropertyDeclaration("Value");
                var getExpression = propertyDeclaration.Getter().ExpressionBody.Expression;
                var setExpression = ((AssignmentExpressionSyntax)propertyDeclaration.Setter().ExpressionBody.Expression).Left;
                var context = new SyntaxNodeAnalysisContext(
                    propertyDeclaration,
                    semanticModel.GetDeclaredSymbol(propertyDeclaration, CancellationToken.None),
                    semanticModel,
                    null,
                    null,
                    null,
                    CancellationToken.None);
                Assert.AreEqual(expected, MemberPath.Uses(setExpression, getExpression, context));
            }

            [TestCase("get => Math.Abs(this.speed - 1) < 1E-2;", "set => this.Speed = 1;", true)]
            [TestCase("get => Math.Abs(this.Speed - 1) < 1E-2;", "set => this.Speed = 1;", true)]
            public void UsesWhenAssigningPropertyAndReturningField(string getter, string setter, bool expected)
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private double speed;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSpeed1
        {
            get => Math.Abs(this.speed - 1) < 1E-2;
            set => this.Speed = 1;
        }

        public double Speed
        {
            get => this.speed;

            set
            {
                if (value.Equals(this.speed))
                {
                    return;
                }

                this.speed = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.IsSpeed1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                testCode = testCode.AssertReplace("get => Math.Abs(this.speed - 1) < 1E-2;", getter)
                                   .AssertReplace("set => this.Speed = 1;", setter);
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var propertyDeclaration = syntaxTree.FindPropertyDeclaration("IsSpeed1");
                var getExpression = propertyDeclaration.Getter().ExpressionBody.Expression;
                var setExpression = ((AssignmentExpressionSyntax)propertyDeclaration.Setter().ExpressionBody.Expression).Left;
                var context = new SyntaxNodeAnalysisContext(
                    propertyDeclaration,
                    semanticModel.GetDeclaredSymbol(propertyDeclaration, CancellationToken.None),
                    semanticModel,
                    null,
                    null,
                    null,
                    CancellationToken.None);
                Assert.AreEqual(expected, MemberPath.Uses(setExpression, getExpression, context));
            }
        }
    }
}
