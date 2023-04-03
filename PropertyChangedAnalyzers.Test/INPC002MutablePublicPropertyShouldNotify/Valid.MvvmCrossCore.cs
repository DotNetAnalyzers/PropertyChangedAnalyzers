namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify;

using Gu.Roslyn.Asserts;
using NUnit.Framework;
using PropertyChangedAnalyzers.Test.Helpers;

public static partial class Valid
{
    public static class MvvmCrossCore
    {
        private static readonly Settings Settings = LibrarySettings.MvvmCross;

        [Test]
        public static void SetProperty()
        {
            var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int p;

        public int P
        {
            get { return p; }
            set { this.SetProperty(ref this.p, value); }
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
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int p;

        public int P
        {
            get => p;
            set => this.SetProperty(ref this.p, value);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code, settings: Settings);
        }

        [TestCase("(string?)null")]
        [TestCase("string.Empty")]
        [TestCase(@"""P""")]
        [TestCase(@"nameof(P)")]
        [TestCase(@"nameof(this.P)")]
        public static void RaisePropertyChanged(string propertyName)
        {
            var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.RaisePropertyChanged(nameof(P));
            }
        }
    }
}".AssertReplace(@"nameof(P)", propertyName);

            RoslynAssert.Valid(Analyzer, Descriptor, new[] { code }, settings: Settings);
        }
    }
}
