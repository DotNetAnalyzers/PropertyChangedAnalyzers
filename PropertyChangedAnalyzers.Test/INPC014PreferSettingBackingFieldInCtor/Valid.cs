namespace PropertyChangedAnalyzers.Test.INPC014PreferSettingBackingFieldInCtor
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AssignmentAnalyzer();

        [Test]
        public static void WhenSettingField()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C(int p)
        {
            this.p = p;
        }

        [DataMember]
        public int P
        {
            get => this.p;
            private set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void AutoProperty()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public C(int p)
        {
            this.P = p;
        }

        [DataMember]
        public int P { get; private set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoreWhenValidationThrows()
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C(int p)
        {
            this.P = p;
        }

        public int P
        {
            get => this.p;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException();
                }

                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoreWhenValidationCall()
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C(int p)
        {
            this.P = p;
        }

        public int P
        {
            get => this.p;
            set
            {
                GreaterThan(value, 0, nameof(value));
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
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
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoreWhenSideEffect()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;
        private int count;

        public event PropertyChangedEventHandler PropertyChanged;

        public C(int p)
        {
            this.P = p;
        }

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
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
            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("(_, __) => this.P = p;")]
        [TestCase("delegate { this.P = p; };")]
        public static void SettingNotifyingPropertyInLambda(string lambda)
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C(string p)
        {
            this.PropertyChanged += (_, __) => this.P = p;
        }

        public string P
        {
            get => this.p;
            private set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("(_, __) => this.P = p;", lambda);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void SettingNotifyingPropertyInLocalFunction()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C(string p)
        {
            void OnChanged(object _, PropertyChangedEventArgs __) => this.P = p;

            this.PropertyChanged += OnChanged;
        }

        public string P
        {
            get => this.p;
            private set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
