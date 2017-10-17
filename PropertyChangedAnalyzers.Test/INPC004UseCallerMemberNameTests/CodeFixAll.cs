﻿namespace PropertyChangedAnalyzers.Test.INPC004UseCallerMemberNameTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFixAll
    {
        [Test]
        public void CallsOnPropertyChanged()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value1;
        private int value2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Sum => this.value1 + this.value2;

        public int Value1
        {
            get
            {
                return this.value1;
            }

            set
            {
                if (value == this.value1)
                {
                    return;
                }

                this.value1 = value;
                this.OnPropertyChanged(↓nameof(Value1));
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

        public int Value2
        {
            get
            {
                return this.value2;
            }

            set
            {
                if (value == this.value2)
                {
                    return;
                }

                this.value2 = value;
                this.OnPropertyChanged(↓nameof(Value2));
                this.OnPropertyChanged(nameof(this.Sum));
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

    public class ViewModel : INotifyPropertyChanged
    {
        private int value1;
        private int value2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Sum => this.value1 + this.value2;

        public int Value1
        {
            get
            {
                return this.value1;
            }

            set
            {
                if (value == this.value1)
                {
                    return;
                }

                this.value1 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

        public int Value2
        {
            get
            {
                return this.value2;
            }

            set
            {
                if (value == this.value2)
                {
                    return;
                }

                this.value2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.FixAll<INPC004UseCallerMemberName, UseCallerMemberNameCodeFixProvider>(testCode, fixedCode);
        }
    }
}