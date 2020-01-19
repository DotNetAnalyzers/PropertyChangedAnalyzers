namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifying
{
    using System.Collections.Immutable;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    public static partial class Valid
    {
        public static class MvvmCrossCore
        {
            private static readonly ImmutableArray<MetadataReference> MetadataReferences = SpecialMetadataReferences.MvvmCross;

            [Test]
            public static void SetAffectsCalculatedProperty()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsCalculatedPropertyEmptyIf()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.SetProperty(ref this.p2, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsSecondCalculatedProperty()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string p3;

        public string P1 => $""Hello {this.P3}"";

        public string P2 => $""Hej {this.P3}"";

        public string P3
        {
            get { return this.p3; }
            set
            {
                if (this.SetProperty(ref this.p3, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                    this.RaisePropertyChanged(nameof(this.P2));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string p3;

        public string P1 => $""Hello {this.P3}"";

        public string P2 => $""Hej {this.P3}"";

        public string P3
        {
            get { return this.p3; }
            set
            {
                if (this.SetProperty(ref this.p3, value))
                {
                    this.RaisePropertyChanged(nameof(this.P1));
                    this.RaisePropertyChanged(nameof(this.P2));
                }
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void RaisePropertyChangedAffectsCalculatedProperty()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string firstName;
        private string lastName;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                if (value == this.firstName)
                {
                    return;
                }

                this.firstName = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.FullName));
            }
        }

        public string LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                if (value == this.lastName)
                {
                    return;
                }

                this.lastName = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.FullName));
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }

            [Test]
            public static void IfNotSetReturnCalculatedProperty()
            {
                var code = @"
namespace N
{
    public class C : MvvmCross.ViewModels.MvxNotifyPropertyChanged
    {
        private string p2;

        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (!SetProperty(ref this.p2, value))
                {
                    return;
                }

                this.RaisePropertyChanged(nameof(this.P1));
            }
        }
    }
}";
                RoslynAssert.Valid(Analyzer, code, metadataReferences: MetadataReferences);
            }
        }
    }
}
