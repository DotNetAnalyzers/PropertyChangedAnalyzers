namespace PropertyChangedAnalyzers.Test.INPC015PropertyIsRecursiveTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(INPC015PropertyIsRecursive.Descriptor);

        [Test]
        public void ExpressionBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Bar => ↓this.Bar;
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void GetterStatementBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Bar
        {
            get { return this.Bar; }
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void GetterExpressionBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Bar
        {
            get => this.Bar;
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void SetterStatementBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int bar;

        public int Bar
        {
            get { return this.bar; }
            set { ↓this.Bar = value; }
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void SetterExpressionBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            set => ↓this.Bar = value;
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }
    }
}
