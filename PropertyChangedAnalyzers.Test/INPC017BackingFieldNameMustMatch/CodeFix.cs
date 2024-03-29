﻿namespace PropertyChangedAnalyzers.Test.INPC017BackingFieldNameMustMatch;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly PropertyDeclarationAnalyzer Analyzer = new();
    private static readonly RenameFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC017BackingFieldNameMisMatch);

    [TestCase("wrong")]
    [TestCase("valuE")]
    [TestCase("valuee")]
    [TestCase("alue")]
    [TestCase("vvalue")]
    public static void ExpressionBody(string fieldName)
    {
        var before = @"
namespace N
{
    public class C
    {
        private int wrong = 1;

        public int P => this.↓wrong;
    }
}".AssertReplace("wrong", fieldName);

        var after = @"
namespace N
{
    public class C
    {
        private int p = 1;

        public int P => this.p;
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("_wrong")]
    [TestCase("_valuE")]
    [TestCase("_pe")]
    [TestCase("_alue")]
    [TestCase("_vvalue")]
    public static void ExpressionBodyUnderscore(string fieldName)
    {
        var before = @"
namespace N
{
    public class C
    {
        private int _wrong = 1;

        public int P => ↓_wrong;
    }
}".AssertReplace("_wrong", fieldName);

        var after = @"
namespace N
{
    public class C
    {
        private int _p = 1;

        public int P => _p;
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("wrong")]
    [TestCase("valuE")]
    [TestCase("valuee")]
    [TestCase("alue")]
    [TestCase("vvalue")]
    public static void ExpressionBodyGetter(string fieldName)
    {
        var before = @"
namespace N
{
    public class C
    {
        private int wrong = 1;

        public int P
        {
            get => this.wrong;
        }
    }
}".AssertReplace("wrong", fieldName);
        var after = @"
namespace N
{
    public class C
    {
        private int p = 1;

        public int P
        {
            get => this.p;
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("wrong")]
    [TestCase("valuE")]
    [TestCase("valuee")]
    [TestCase("alue")]
    [TestCase("vvalue")]
    public static void StatementBodyGetter(string fieldName)
    {
        var before = @"
namespace N
{
    public class C
    {
        private int wrong = 1;

        public int P
        {
            get { return this.wrong; }
        }
    }
}".AssertReplace("wrong", fieldName);

        var after = @"
namespace N
{
    public class C
    {
        private int p = 1;

        public int P
        {
            get { return this.p; }
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("_wrong")]
    [TestCase("_valuE")]
    [TestCase("_pe")]
    [TestCase("_alue")]
    [TestCase("_vvalue")]
    public static void ExpressionBodyGetterUnderscore(string fieldName)
    {
        var before = @"
namespace N
{
    public class C
    {
        private int _wrong = 1;

        public int P
        {
            get => ↓_wrong;
        }
    }
}".AssertReplace("_wrong", fieldName);
        var after = @"
namespace N
{
    public class C
    {
        private int _p = 1;

        public int P
        {
            get => _p;
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
