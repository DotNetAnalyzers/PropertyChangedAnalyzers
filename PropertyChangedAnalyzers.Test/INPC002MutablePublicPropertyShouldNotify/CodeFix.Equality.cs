namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify
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
            [TestCase("string?")]
            [TestCase("StringComparison")]
            public static void OpEqualsFor(string typeCode)
            {
                var before = @"
namespace N
{
    using System;
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int ↓P { get; set; }

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

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

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
                this.OnPropertyChanged(nameof(this.P));
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
                var referenceType = @"
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

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ReferenceType? ↓P { get; set; }

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

    public class C : INotifyPropertyChanged
    {
        private ReferenceType? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ReferenceType? P
        {
            get => this.p;
            set
            {
                if (ReferenceEquals(value, this.p))
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { referenceType, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { referenceType, before }, after);
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
        public readonly int P;

        public EquatableStruct(int p)
        {
            this.P = p;
        }

        public bool Equals(EquatableStruct other)
        {
            return this.P == other.P;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EquatableStruct && this.Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.P;
        }
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public EquatableStruct ↓P { get; set; }

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

    public class C : INotifyPropertyChanged
    {
        private EquatableStruct p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public EquatableStruct P
        {
            get => this.p;
            set
            {
                if (value.Equals(this.p))
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
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
        public readonly int P;

        public EquatableStruct(int p)
        {
            this.P = p;
        }

        public bool Equals(EquatableStruct other)
        {
            return this.P == other.P;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EquatableStruct && this.Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.P;
        }
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public EquatableStruct? ↓P { get; set; }

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

    public class C : INotifyPropertyChanged
    {
        private EquatableStruct? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public EquatableStruct? P
        {
            get => this.p;
            set
            {
                if (System.Nullable.Equals(value, this.p))
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
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
        public readonly int P;


        public EquatableStruct(int p)
        {
            this.P = p;
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
            return this.P == other.P;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EquatableStruct && this.Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.P;
        }
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public EquatableStruct ↓P { get; set; }

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

    public class C : INotifyPropertyChanged
    {
        private EquatableStruct p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public EquatableStruct P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
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
        public readonly int P;

        public EquatableStruct(int p)
        {
            this.P = p;
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
            return this.P == other.P;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EquatableStruct && this.Equals((EquatableStruct)obj);
        }

        public override int GetHashCode()
        {
            return this.P;
        }
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public EquatableStruct? ↓P { get; set; }

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

    public class C : INotifyPropertyChanged
    {
        private EquatableStruct? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public EquatableStruct? P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
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
                var notEquatableStruct = @"
namespace N
{
    public struct NotEquatableStruct
    {
        public readonly int P;

        public NotEquatableStruct(int p)
        {
            this.P = p;
        }
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public NotEquatableStruct ↓P { get; set; }

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

    public class C : INotifyPropertyChanged
    {
        private NotEquatableStruct p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public NotEquatableStruct P
        {
            get => this.p;
            set
            {
                if (System.Collections.Generic.EqualityComparer<NotEquatableStruct>.Default.Equals(value, this.p))
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { notEquatableStruct, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { notEquatableStruct, before }, after);
            }

            [Test]
            public static void NullableNotEquatableStruct()
            {
                var notEquatableStruct = @"
namespace N
{
    public struct NotEquatableStruct
    {
        public readonly int P;

        public NotEquatableStruct(int p)
        {
            this.P = p;
        }
    }
}";
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public NotEquatableStruct? ↓P { get; set; }

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

    public class C : INotifyPropertyChanged
    {
        private NotEquatableStruct? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public NotEquatableStruct? P
        {
            get => this.p;
            set
            {
                if (System.Nullable.Equals(value, this.p))
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { notEquatableStruct, before }, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { notEquatableStruct, before }, after);
            }
        }
    }
}
