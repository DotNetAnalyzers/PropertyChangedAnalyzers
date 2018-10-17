namespace PropertyChangedAnalyzers.Test.INPC014PreferSettingBackingFieldInCtorTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly INPC014PreferSettingBackingFieldInCtor Analyzer = new INPC014PreferSettingBackingFieldInCtor();

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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void AutoProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(int value)
        {
            this.Value = value;
        }

        [DataMember]
        public int Value { get; private set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoreWhenValidationThrows()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(int value)
        {
            this.Value = value;
        }

        public int Value
        {
            get => this.value;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException();
                }

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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoreWhenValidationCall()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(int value)
        {
            this.Value = value;
        }

        public int Value
        {
            get => this.value;
            set
            {
                GreaterThan(value, 0, nameof(value));
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

        private static void GreaterThan<T>(T value, T min, string parameterName)
            where T : IComparable<T>
        {
            Debug.Assert(!string.IsNullOrEmpty(parameterName), nameof(parameterName));
            if (Comparer<T>.Default.Compare(value, min) <= 0)
            {
                string message = $""Expected {parameterName} to be greater than {min}, {parameterName} was {value}"";
                throw new ArgumentException(message, parameterName);
            }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoreWhenSideEffect()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private int value;
        private int count;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(int value)
        {
            this.Value = value;
        }

        public int Value
        {
            get => this.value;
            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.count++;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
