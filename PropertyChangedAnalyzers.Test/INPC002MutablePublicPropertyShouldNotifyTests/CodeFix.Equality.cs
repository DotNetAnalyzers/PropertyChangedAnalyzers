namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class CodeFixEquality
        {
            [TestCase("int")]
            [TestCase("int?")]
            [TestCase("Nullable<int>")]
            [TestCase("string")]
            [TestCase("StringComparison")]
            public void OpEqualsFor(string typeCode)
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                testCode = testCode.AssertReplace("int", typeCode);

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(nameof(this.Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                fixedCode = fixedCode.AssertReplace("int", typeCode);
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void ReferenceType()
            {
                var refTypeCode = @"
namespace RoslynSandbox
{
    public class ReferenceType
    {
    }
}";
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public ReferenceType Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        private ReferenceType bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType Bar
        {
            get => this.bar;
            set
            {
                if (ReferenceEquals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(nameof(this.Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { refTypeCode, testCode }, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { refTypeCode, testCode }, fixedCode);
            }

            [Test]
            public void EquatableStruct()
            {
                var equatableStruct = @"
namespace RoslynSandbox
{
    using System;

    public struct EquatableStruct : IEquatable<EquatableStruct>
    {
        public readonly int Value;

        public EquatableStruct(int value)
        {
            this.Value = value;
        }

        public bool Equals(EquatableStruct other)
        {
            return this.Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EquatableStruct && Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.Value;
        }
    }
}";
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public EquatableStruct Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        private EquatableStruct bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public EquatableStruct Bar
        {
            get => this.bar;
            set
            {
                if (value.Equals(this.bar))
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(nameof(this.Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
            }

            [Test]
            public void NullableEquatableStruct()
            {
                var equatableStruct = @"
namespace RoslynSandbox
{
    using System;

    public struct EquatableStruct : IEquatable<EquatableStruct>
    {
        public readonly int Value;

        public EquatableStruct(int value)
        {
            this.Value = value;
        }

        public bool Equals(EquatableStruct other)
        {
            return this.Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EquatableStruct && Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.Value;
        }
    }
}";
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public EquatableStruct? Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        private EquatableStruct? bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public EquatableStruct? Bar
        {
            get => this.bar;
            set
            {
                if (System.Nullable.Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(nameof(this.Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
            }

            [Test]
            public void EquatableStructWithOpEquals()
            {
                var equatableStruct = @"
namespace RoslynSandbox
{
    using System;

    public struct EquatableStruct : IEquatable<EquatableStruct>
    {
        public readonly int Value;


        public EquatableStruct(int value)
        {
            this.Value = value;
        }

        public static bool operator ==(EquatableStruct left, EquatableStruct right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EquatableStruct left, EquatableStruct right)
        {
            return !left.Equals(right);
        }

        public bool Equals(EquatableStruct other)
        {
            return this.Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EquatableStruct && Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.Value;
        }
    }
}";
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public EquatableStruct Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        private EquatableStruct bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public EquatableStruct Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(nameof(this.Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
            }

            [Test]
            public void NullableEquatableStructOpEquals()
            {
                var equatableStruct = @"
namespace RoslynSandbox
{
    using System;

    public struct EquatableStruct : IEquatable<EquatableStruct>
    {
        public readonly int Value;

        public EquatableStruct(int value)
        {
            this.Value = value;
        }

        public static bool operator ==(EquatableStruct left, EquatableStruct right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EquatableStruct left, EquatableStruct right)
        {
            return !left.Equals(right);
        }

        public bool Equals(EquatableStruct other)
        {
            return this.Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EquatableStruct && Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.Value;
        }
    }
}";
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public EquatableStruct? Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        private EquatableStruct? bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public EquatableStruct? Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(nameof(this.Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
            }

            [Test]
            public void NotEquatableStruct()
            {
                var equatableStruct = @"
namespace RoslynSandbox
{
    public struct NotEquatableStruct
    {
        public readonly int Value;

        public NotEquatableStruct(int value)
        {
            this.Value = value;
        }
    }
}";
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public NotEquatableStruct Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        private NotEquatableStruct bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public NotEquatableStruct Bar
        {
            get => this.bar;
            set
            {
                if (System.Collections.Generic.EqualityComparer<NotEquatableStruct>.Default.Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(nameof(this.Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
            }

            [Test]
            public void NullableNotEquatableStruct()
            {
                var equatableStruct = @"
namespace RoslynSandbox
{
    public struct NotEquatableStruct
    {
        public readonly int Value;

        public NotEquatableStruct(int value)
        {
            this.Value = value;
        }
    }
}";
                var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public NotEquatableStruct? Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        private NotEquatableStruct? bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public NotEquatableStruct? Bar
        {
            get => this.bar;
            set
            {
                if (System.Nullable.Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(nameof(this.Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, testCode }, fixedCode);
            }
        }
    }
}
