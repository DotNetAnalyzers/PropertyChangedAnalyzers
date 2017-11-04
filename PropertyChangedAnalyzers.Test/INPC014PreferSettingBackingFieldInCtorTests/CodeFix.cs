namespace PropertyChangedAnalyzers.Test.INPC014PreferSettingBackingFieldInCtorTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        [Test]
        public void WhenSettingField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(int value)
        {
            ↓this.Value = value;
        }

        [DataMember]
        public int Value
        {
            get => this.value;
            private set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(int value)
        {
            this.value = value;
        }

        [DataMember]
        public int Value
        {
            get => this.value;
            private set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.CodeFix<INPC014PreferSettingBackingFieldInCtor, SetBackingFieldCodeFix>(testCode, fixedCode);
        }
    }
}
