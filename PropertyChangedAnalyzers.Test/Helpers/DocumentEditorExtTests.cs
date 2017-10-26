namespace PropertyChangedAnalyzers.Test
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;
    using NUnit.Framework;

    public class DocumentEditorExtTests
    {
        [Test]
        public async Task AddBackingFieldWhenNoBackingFields()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Meh1 = 1;
        private int meh2;

        public int Value { get; set; }
    }
}";
            var sln = CodeFactory.CreateSolution(testCode);
            var editor = await DocumentEditor.CreateAsync(sln.Projects.First().Documents.First()).ConfigureAwait(false);
            var property = editor.OriginalRoot.SyntaxTree.FindPropertyDeclaration("Value");
            var field = editor.AddBackingField(property, usesUnderscoreNames: false, cancellationToken: CancellationToken.None);
            Assert.AreEqual("privateint value;", field.ToFullString());
            var expected = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Meh1 = 1;
        private int meh2;
        private int value;

        public int Value { get; set; }
    }
}";
            CodeAssert.AreEqual(expected, editor.GetChangedDocument());
        }

        [Test]
        public async Task AddBackingFieldWhenNameCollision()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int value;

        public int Value { get; set; }
    }
}";
            var sln = CodeFactory.CreateSolution(testCode);
            var editor = await DocumentEditor.CreateAsync(sln.Projects.First().Documents.First()).ConfigureAwait(false);
            var property = editor.OriginalRoot.SyntaxTree.FindPropertyDeclaration("Value");
            var field = editor.AddBackingField(property, usesUnderscoreNames: false, cancellationToken: CancellationToken.None);
            Assert.AreEqual("privateint value_;", field.ToFullString());
            var expected = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int value;
        private int value_;

        public int Value { get; set; }
    }
}";
            CodeAssert.AreEqual(expected, editor.GetChangedDocument());
        }

        [Test]
        public async Task AddPrivateMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public abstract class Foo
    {
        public int Filed1 = 1;
        private int filed1;

        public Foo()
        {
        }

        private Foo(int i)
        {
        }

        public int Prop1 { get; set; }

        public void Bar1()
        {
        }

        internal void Bar2()
        {
        }

        protected void Bar3()
        {
        }

        private static void Bar4()
        {
        }

        private void Bar5()
        {
        }
    }
}";
            var sln = CodeFactory.CreateSolution(testCode);
            var editor = await DocumentEditor.CreateAsync(sln.Projects.First().Documents.First()).ConfigureAwait(false);
            var containingType = editor.OriginalRoot.SyntaxTree.FindClassDeclaration("Foo");
            var method = SyntaxFactory.ParseCompilationUnit("private int NewMethod() => 1;")
                                      .Members
                                      .Single()
                                      .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                                      .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                      .WithAdditionalAnnotations(Formatter.Annotation);

            var expected = @"
namespace RoslynSandbox
{
    public abstract class Foo
    {
        public int Filed1 = 1;
        private int filed1;

        public Foo()
        {
        }

        private Foo(int i)
        {
        }

        public int Prop1 { get; set; }

        public void Bar1()
        {
        }

        internal void Bar2()
        {
        }

        protected void Bar3()
        {
        }

        private static void Bar4()
        {
        }

        private void Bar5()
        {
        }

        private int NewMethod() => 1;
    }
}";
            editor.AddMethod(containingType, (MethodDeclarationSyntax)method);
            CodeAssert.AreEqual(expected, editor.GetChangedDocument());
        }

        [Test]
        public async Task AddPublicMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public abstract class Foo
    {
        public int Filed1 = 1;
        private int filed1;

        public Foo()
        {
        }

        private Foo(int i)
        {
        }

        public int Prop1 { get; set; }

        public void Bar1()
        {
        }

        internal void Bar2()
        {
        }

        protected void Bar3()
        {
        }

        private static void Bar4()
        {
        }

        private void Bar5()
        {
        }
    }
}";
            var sln = CodeFactory.CreateSolution(testCode);
            var editor = await DocumentEditor.CreateAsync(sln.Projects.First().Documents.First()).ConfigureAwait(false);
            var containingType = editor.OriginalRoot.SyntaxTree.FindClassDeclaration("Foo");
            var method = SyntaxFactory.ParseCompilationUnit("public int NewMethod() => 1;")
                                      .Members
                                      .Single()
                                      .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                                      .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                      .WithAdditionalAnnotations(Formatter.Annotation);

            var expected = @"
namespace RoslynSandbox
{
    public abstract class Foo
    {
        public int Filed1 = 1;
        private int filed1;

        public Foo()
        {
        }

        private Foo(int i)
        {
        }

        public int Prop1 { get; set; }

        public void Bar1()
        {
        }

        public int NewMethod() => 1;

        internal void Bar2()
        {
        }

        protected void Bar3()
        {
        }

        private static void Bar4()
        {
        }

        private void Bar5()
        {
        }
    }
}";
            editor.AddMethod(containingType, (MethodDeclarationSyntax)method);
            CodeAssert.AreEqual(expected, editor.GetChangedDocument());
        }
    }
}