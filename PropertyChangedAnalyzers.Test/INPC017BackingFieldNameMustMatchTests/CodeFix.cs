namespace PropertyChangedAnalyzers.Test.INPC017BackingFieldNameMustMatchTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new RenameFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(INPC017BackingFieldNameMustMatch.Descriptor);

        [TestCase("wrong")]
        [TestCase("valuE")]
        [TestCase("valuee")]
        [TestCase("alue")]
        [TestCase("vvalue")]
        public void ExpressionBody(string fieldName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int wrong;

        public int Value => this.↓wrong;
    }
}".AssertReplace("wrong", fieldName);

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int value;

        public int Value => this.value;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [TestCase("_wrong")]
        [TestCase("_valuE")]
        [TestCase("_valuee")]
        [TestCase("_alue")]
        [TestCase("_vvalue")]
        public void ExpressionBodyUnderscore(string fieldName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int _wrong;

        public int Value => ↓_wrong;
    }
}".AssertReplace("_wrong", fieldName);

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int _value;

        public int Value => _value;
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [TestCase("wrong")]
        [TestCase("valuE")]
        [TestCase("valuee")]
        [TestCase("alue")]
        [TestCase("vvalue")]
        public void ExpressionBodyGetter(string fieldName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int wrong;

        public int Value
        {
            get => this.wrong;
        }
    }
}".AssertReplace("wrong", fieldName);
            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int value;

        public int Value
        {
            get => this.value;
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [TestCase("wrong")]
        [TestCase("valuE")]
        [TestCase("valuee")]
        [TestCase("alue")]
        [TestCase("vvalue")]
        public void StatementBodyGetter(string fieldName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int wrong;

        public int Value
        {
            get { return this.wrong; }
        }
    }
}".AssertReplace("wrong", fieldName);

            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int value;

        public int Value
        {
            get { return this.value; }
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [TestCase("_wrong")]
        [TestCase("_valuE")]
        [TestCase("_valuee")]
        [TestCase("_alue")]
        [TestCase("_vvalue")]
        public void ExpressionBodyGetterUnderscore(string fieldName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int _wrong;

        public int Value
        {
            get => ↓_wrong;
        }
    }
}".AssertReplace("_wrong", fieldName);
            var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int _value;

        public int Value
        {
            get => _value;
        }
    }
}";

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
