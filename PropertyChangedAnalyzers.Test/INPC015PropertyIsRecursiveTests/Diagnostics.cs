namespace PropertyChangedAnalyzers.Test.INPC015PropertyIsRecursiveTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC015PropertyIsRecursive);

        [Test]
        public static void ExpressionBody()
        {
            var code = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public int Bar => ↓this.Bar;
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void GetterStatementBody()
        {
            var code = @"
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void GetterExpressionBody()
        {
            var code = @"
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void SetterStatementBody()
        {
            var code = @"
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void SetterExpressionBody()
        {
            var code = @"
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
