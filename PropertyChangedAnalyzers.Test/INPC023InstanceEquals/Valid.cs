namespace PropertyChangedAnalyzers.Test.INPC023InstanceEquals
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly SetAccessorAnalyzer Analyzer = new();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC023InstanceEquals;

        [Test]
        public static void ValueType()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int P
        {
            get => this.p;
            set
            {
                if (value.Equals(this.p))
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
