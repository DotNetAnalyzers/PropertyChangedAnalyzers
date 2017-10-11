﻿namespace PropertyChangedAnalyzers.Test.INPC001ImplementINotifyPropertyChangedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class MvvmLight
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.AddTransitiveMetadataReferences(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetAll();
            }

            [Test]
            public void SubclassViewModelBase()
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
                AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode, "Subclass GalaSoft.MvvmLight.ViewModelBase", AllowCompilationErrors.Yes);
            }

            [Test]
            public void ImplementINotifyPropertyChanged()
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
                AnalyzerAssert.CodeFix<INPC001ImplementINotifyPropertyChanged, ImplementINotifyPropertyChangedCodeFixProvider>(testCode, fixedCode, "Implement INotifyPropertyChanged.", AllowCompilationErrors.Yes);
            }
        }
    }
}