namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
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
            public void MvvmLight()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : GalaSoft.MvvmLight.ViewModelBase
    {
        public int Bar { get; set; }
    }
}";
                AnalyzerAssert.MetadataReferences.Add(MetadataReference.CreateFromFile(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly.Location));
                AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode, "Subclass GalaSoft.MvvmLight.ViewModelBase", AllowCompilationErrors.Yes);
            }
        }
    }
}