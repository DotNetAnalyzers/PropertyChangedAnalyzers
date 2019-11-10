namespace PropertyChangedAnalyzers.Test.INPC013UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ArgumentAnalyzer();

        [Test]
        public static void WhenThrowingArgumentException()
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
                throw new ArgumentNullException(nameof(value));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

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
    [System.Diagnostics.DebuggerDisplay(""{Name}"")]
    public class C
    {
        public string Name { get; }
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

        public void M2(string value)
        {
            throw new ArgumentException(nameof(value), value);
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

        public string M2(string value)
        {
            throw new ArgumentException(nameof(value), value);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoresNamespaceName()
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

        public void M2(string value)
        {
            throw new ArgumentException(nameof(value), value);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
