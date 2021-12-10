namespace PropertyChangedAnalyzers.Test.INPC009NotifiesForMissingProperty
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class Diagnostics
    {
        public static class Invocation
        {
            private static readonly InvocationAnalyzer Analyzer = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC009NotifiesForMissingProperty);

            [Test]
            public static void CallsCallerMemberNameFromMethod()
            {
                var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void M()
        {
            ↓this.OnPropertyChanged();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
            }
        }
    }
}
