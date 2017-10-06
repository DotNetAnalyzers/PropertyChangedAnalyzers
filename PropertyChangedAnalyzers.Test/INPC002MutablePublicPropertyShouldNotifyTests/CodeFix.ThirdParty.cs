namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class ThirdParty
        {
            [TearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetMetadataReferences();
            }

            [Test]
            public void AutoPropertyMvvmFramework()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using MvvmFramework;

    public class Foo : ViewModelBase
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using MvvmFramework;

    public class Foo : ViewModelBase
    {
        private int bar;

        public int Bar
        {
            get
            {
                return this.bar;
            }

            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged();
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC002MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>(testCode, fixedCode);
            }
        }
    }
}