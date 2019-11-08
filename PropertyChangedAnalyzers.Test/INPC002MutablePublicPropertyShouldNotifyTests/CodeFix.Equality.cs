namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class Equality
        {
            [TestCase("int")]
            [TestCase("int?")]
            [TestCase("Nullable<int>")]
            [TestCase("string")]
            [TestCase("StringComparison")]
            public static void OpEqualsFor(string typeCode)
            {
                var before = @"
namespace N
{
    using System;
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("int", typeCode);

                var after = @"
namespace N
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
}".AssertReplace("int", typeCode);
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void ReferenceType()
            {
                var refTypeCode = @"
namespace N
{
    public class ReferenceType
    {
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ReferenceType ↓Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { refTypeCode, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { refTypeCode, before }, after);
            }

            [Test]
            public static void EquatableStruct()
            {
                var equatableStruct = @"
namespace N
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
            return obj is EquatableStruct && this.Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.Value;
        }
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public EquatableStruct ↓Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
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

                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
            }

            [Test]
            public static void NullableEquatableStruct()
            {
                var equatableStruct = @"
namespace N
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
            return obj is EquatableStruct && this.Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.Value;
        }
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public EquatableStruct? ↓Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
            }

            [Test]
            public static void EquatableStructWithOpEquals()
            {
                var equatableStruct = @"
namespace N
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
            return obj is EquatableStruct && this.Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.Value;
        }
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public EquatableStruct ↓Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
            }

            [Test]
            public static void NullableEquatableStructOpEquals()
            {
                var equatableStruct = @"
namespace N
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
            return obj is EquatableStruct && this.Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.Value;
        }
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public EquatableStruct? ↓Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
            }

            [Test]
            public static void NotEquatableStruct()
            {
                var equatableStruct = @"
namespace N
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
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NotEquatableStruct ↓Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
            }

            [Test]
            public static void NullableNotEquatableStruct()
            {
                var equatableStruct = @"
namespace N
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
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NotEquatableStruct? ↓Bar { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { equatableStruct, before }, after);
            }
        }
    }
}
