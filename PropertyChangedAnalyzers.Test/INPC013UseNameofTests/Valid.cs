namespace PropertyChangedAnalyzers.Test.INPC013UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ArgumentAnalyzer();

        [Test]
        public static void ArgumentOutOfRangeException()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public void M(StringComparison value)
        {
            switch (value)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresDebuggerDisplay()
        {
            var code = @"
namespace N
{
    [System.Diagnostics.DebuggerDisplay(""{P}"")]
    public class C
    {
        public string P { get; }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresTypeName()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public void M1()
        {
            this.M2(""Exception"");
        }

        public void M2(string s)
        {
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresVariableDeclaredAfter()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public void M1()
        {
            var text = this.M2(""text"");
        }

        public string M2(string s) => string.Empty;
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoreNamespaceName()
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public void M1()
        {
            this.M2(""Test"");
        }

        public void M2(string p)
        {
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("\"Number\"")]
        [TestCase("nameof(Number)")]
        public static void IgnoreDependencyProperty(string expression)
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class WpfControl : Control
    {
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            ""Number"",
            typeof(int),
            typeof(WpfControl),
            new PropertyMetadata(default(int)));

        public int Number
        {
            get => (int)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }
    }
}".AssertReplace("\"Number\"", expression);

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("\"value\"")]
        [TestCase("nameof(value)")]
        public static void IgnoreArgumentException(string expression)
        {
            var code = @"
namespace N
{
    using System;

    public class C
    {
        public void M(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(""value"");
            }
        }
    }
}".AssertReplace("\"value\"", expression);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoreArbitraryInvocation1()
        {
            var code = @"
namespace N
{
    public class C
    {
        public int P { get; set; }

        public static void M1()
        {
            M1(""P"");
        }

        public static void M1(string s)
        {
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoreArbitraryInvocation2()
        {
            var code = @"
namespace N
{
    public class C
    {
        public static readonly string F = M(""P"");

        public int P { get; set; }

        public static string M(string s) => s;
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoreStringFormat()
        {
            var code = @"
namespace N
{
    public class C
    {
        public readonly string F = string.Format(""P"");

        private int p;

        public int P
        {
            get { return this.p; }
            set { this.p = value; }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
