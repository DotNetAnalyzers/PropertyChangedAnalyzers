namespace PropertyChangedAnalyzers.Test.INPC017BackingFieldNameMustMatchTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("INPC017");

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

        public int Value => ↓this.wrong;
    }
}";
            testCode = testCode.AssertReplace("wrong", fieldName);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
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
}";
            testCode = testCode.AssertReplace("_wrong", fieldName);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
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
}";
            testCode = testCode.AssertReplace("wrong", fieldName);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
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
}";
            testCode = testCode.AssertReplace("wrong", fieldName);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
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
            get => _wrong;
        }
    }
}";
            testCode = testCode.AssertReplace("_wrong", fieldName);
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }
    }
}
