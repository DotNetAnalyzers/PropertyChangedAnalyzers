namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using System.Collections.Generic;
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodFix
    {
        internal class WhenError
        {
            public static readonly IReadOnlyList<EqualsItem> EqualsSource = new[]
            {
                new EqualsItem("string", "Equals(value, this.bar)"),
                new EqualsItem("string", "Equals(this.bar, value)"),
                new EqualsItem("string", "Equals(value, bar)"),
                new EqualsItem("string", "Equals(value, Bar)"),
                new EqualsItem("string", "Equals(Bar, value)"),
                new EqualsItem("string", "Nullable.Equals(value, this.bar)"),
                new EqualsItem("int?", "Nullable.Equals(value, this.bar)"),
                new EqualsItem("string", "value.Equals(this.bar)"),
                new EqualsItem("string", "value.Equals(bar)"),
                new EqualsItem("string", "this.bar.Equals(value)"),
                new EqualsItem("string", "bar.Equals(value)"),
                new EqualsItem("string", "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.bar)"),
                new EqualsItem("string", "ReferenceEquals(value, this.bar)"),
            };

            [Test]
            public void OperatorNotEquals()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value != this.bar)
                {
                    return;
                }

                this.bar = value;
                ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

                AnalyzerAssert.NoFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(
                    testCode);
            }

            [Test]
            public void OperatorEquals()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    this.bar = value;
                    ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

                AnalyzerAssert.NoFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode);
            }

            [TestCaseSource(nameof(EqualsSource))]
            public void Check(EqualsItem check)
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Bar
        {
            get { return this.bar; }
            set
            {
                if (Equals(value, this.bar))
                {
                    this.bar = value;
                    ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
                testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call)
                                   .AssertReplace("string", check.Type);
                AnalyzerAssert.NoFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode);
            }

            [TestCaseSource(nameof(EqualsSource))]
            public void NegatedCheck(EqualsItem check)
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Bar
        {
            get { return this.bar; }
            set
            {
                if (!Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";
                testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call)
                                   .AssertReplace("string", check.Type);
                AnalyzerAssert.NoFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(
                    testCode);
            }

            public class EqualsItem
            {
                public EqualsItem(string type, string call)
                {
                    this.Type = type;
                    this.Call = call;
                }

                internal string Type { get; }

                internal string Call { get; }

                public override string ToString()
                {
                    return $"{nameof(this.Type)}: {this.Type}, {nameof(this.Call)}: {this.Call}";
                }
            }
        }
    }
}