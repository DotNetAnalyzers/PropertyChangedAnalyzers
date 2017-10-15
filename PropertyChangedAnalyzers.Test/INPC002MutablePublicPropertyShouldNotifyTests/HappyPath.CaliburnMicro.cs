﻿namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        internal class CaliburnMicro
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.AddTransitiveMetadataReferences(typeof(Caliburn.Micro.PropertyChangedBase).Assembly);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetAll();
            }

            [Test]
            public void Set()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        private int value;

        public int Value
        {
            get { return value; }
            set { this.Set(ref this.value, value); }
        }
    }
}";

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [Test]
            public void SetExpressionBodies()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : Caliburn.Micro.PropertyChangedBase
    {
        private int value;

        public int Value
        {
            get => return value;
            set => this.SetValue(ref this.value, value);
        }
    }
}";

                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }

            [TestCase("null")]
            [TestCase("string.Empty")]
            [TestCase(@"""Bar""")]
            [TestCase(@"nameof(Bar)")]
            [TestCase(@"nameof(this.Bar)")]
            public void RaisePropertyChanged(string propertyName)
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Caliburn.Micro.PropertyChangedBase
    {
        private int bar;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.NotifyOfPropertyChange(nameof(Bar));
            }
        }
    }
}";

                testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
                AnalyzerAssert.Valid<INPC002MutablePublicPropertyShouldNotify>(testCode);
            }
        }
    }
}