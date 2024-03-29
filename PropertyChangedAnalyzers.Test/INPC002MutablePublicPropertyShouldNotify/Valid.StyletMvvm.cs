﻿namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify;

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using PropertyChangedAnalyzers.Test.Helpers;

public partial class Valid
{
    public static class StyletMvvm
    {
        private static readonly Settings Settings = LibrarySettings.Stylet;

        [Test]
        public static void Set()
        {
            var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private int p;

        public int P
        {
            get { return p; }
            set { this.SetAndNotify(ref this.p, value); }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, settings: Settings);
        }

        [Test]
        public static void SetExpressionBodies()
        {
            var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private int p;

        public int P
        {
            get => p;
            set => this.SetAndNotify(ref this.p, value);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, settings: Settings);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""P""")]
        [TestCase(@"nameof(P)")]
        [TestCase(@"nameof(this.P)")]
        public static void RaisePropertyChanged(string propertyName)
        {
            var code = @"
namespace N
{
    public class C : Stylet.PropertyChangedBase
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.NotifyOfPropertyChange(nameof(P));
            }
        }
    }
}".AssertReplace(@"nameof(P)", propertyName);

            RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, settings: Settings);
        }
    }
}
