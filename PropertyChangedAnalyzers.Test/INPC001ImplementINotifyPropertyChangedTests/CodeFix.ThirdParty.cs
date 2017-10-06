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
            public void MvvmLightSubclassViewModelBase()
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

            [Test]
            public void MvvmLightSubclassViewModelBaseWhenINPC()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : INotifyPropertyChanged
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

            [Test]
            public void MvvmLightImplementINotifyPropertyChanged()
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
    public class Foo : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public int Bar { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.MetadataReferences.Add(MetadataReference.CreateFromFile(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly.Location));
                AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode, "Implement INotifyPropertyChanged.", AllowCompilationErrors.Yes);
            }
        }
    }
}