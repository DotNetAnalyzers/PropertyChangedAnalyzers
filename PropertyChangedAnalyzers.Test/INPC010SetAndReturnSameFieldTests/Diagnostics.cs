namespace PropertyChangedAnalyzers.Test.INPC010SetAndReturnSameFieldTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        [Test]
        public void GetterReturnsOtherThanSetterAssigns()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int otherValue;
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get
            {
                return this.otherValue;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged(nameof(Value));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Diagnostics<INPC010SetAndReturnSameField>(testCode);
        }
    }
}