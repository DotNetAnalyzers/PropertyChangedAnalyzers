﻿namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify;

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using PropertyChangedAnalyzers.Test.Helpers;

public partial class Valid
{
    public static class PrismBindableBase
    {
        private static readonly Settings MetadataReferences = LibrarySettings.Prism;

        [Test]
        public static void SetProperty()
        {
            var code = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private int p;

        public int P
        {
            get { return p; }
            set { this.SetProperty(ref this.p, value); }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, settings: MetadataReferences);
        }

        [Test]
        public static void SetPropertyExpressionBodies()
        {
            var code = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private int p;

        public int P
        {
            get => p;
            set => this.SetProperty(ref this.p, value);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code, settings: MetadataReferences);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""P""")]
        [TestCase(@"nameof(P)")]
        [TestCase(@"nameof(this.P)")]
        public static void OnPropertyChanged(string propertyName)
        {
            var code = @"
namespace N
{
    public class C : Microsoft.Practices.Prism.Mvvm.BindableBase
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
            }
        }
    }
}".AssertReplace(@"nameof(this.P)", propertyName);

            RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, settings: MetadataReferences);
        }
    }
}
