namespace PropertyChangedAnalyzers.Test.INPC011DoNotShadow
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new EventAnalyzer();

        [Test]
        public static void NoBaseClass()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(P));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void OverridingEvent()
        {
            var viewModelBaseCode = @"
namespace N.Core
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModelBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : N.Core.ViewModelBase
    {
        public override event PropertyChangedEventHandler PropertyChanged;
    }
}";

            RoslynAssert.Valid(Analyzer, viewModelBaseCode, code);
        }
    }
}
