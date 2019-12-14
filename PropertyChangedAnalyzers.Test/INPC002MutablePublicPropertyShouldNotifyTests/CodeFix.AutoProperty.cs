namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class AutoProperty
        {
            [Test]
            public static void Message()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                var expectedDiagnostic = ExpectedDiagnostic.WithMessage("Property 'P' should notify when value changes.");
                RoslynAssert.CodeFix(Analyzer, Fix, expectedDiagnostic, before, after, fixTitle: "Notify when value changes.");
            }

            [Test]
            public static void CallerMemberName()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void InternalClassInternalPropertyCallerMemberName()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal int ↓P { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void ExplicitName()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void PropertyChangedEventArgs()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.P)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void PropertyChangedEventArgsBeforeCallerMemberName()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void CallerMemberNameBeforePropertyChangedEventArgs()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [TestCaseSource(typeof(Code), nameof(Code.AutoDetectedStyles))]
            public static void CallerMemberNameStyleDetection(AutoDetectedStyle style)
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private readonly int p1;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p1;

        public int ↓P2 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private readonly int p1;
        private int p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p1;

        public int P2
        {
            get => this.p2;
            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                RoslynAssert.CodeFix(
                    Analyzer,
                    Fix,
                    ExpectedDiagnostic,
                    new[] { style.AdditionalSample, style.Apply(before, "p1") },
                    style.Apply(after, "p1", "p2"));
            }

            [Test]
            public static void WithTrivia()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Some text
        /// </summary>
        public int ↓P { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Some text
        /// </summary>
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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void Vanilla()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void SeparateLines()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get;
            set;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void Virtual()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual int ↓P { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void PrivateSetMutatedOutsideConstructor()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
         public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; private set; }

        public void Mutate()
        {
            this.P++;
        }

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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

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
                this.OnPropertyChanged(nameof(this.P));
            }
        }

        public void Mutate()
        {
            this.P++;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void Initialized()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool ↓IsTrue { get; set; } = true;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private bool isTrue = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsTrue
        {
            get => this.isTrue;
            set
            {
                if (value == this.isTrue)
                {
                    return;
                }

                this.isTrue = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void Generic()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public T ↓P { get; set; }

        protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;

    public class C<T> : INotifyPropertyChanged
    {
        private T p;

        public event PropertyChangedEventHandler PropertyChanged;

        public T P
        {
            get => this.p;
            set
            {
                if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(value, this.p))
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void ConstrainedGenericReferenceType()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C<T> : INotifyPropertyChanged
        where T : class
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public T ↓P { get; set; }

        protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;

    public class C<T> : INotifyPropertyChanged
        where T : class
    {
        private T p;

        public event PropertyChangedEventHandler PropertyChanged;

        public T P
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

        protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void ConstrainedGenericValueType()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C<T> : INotifyPropertyChanged
        where T : struct
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public T ↓P { get; set; }

        protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;

    public class C<T> : INotifyPropertyChanged
        where T : struct
    {
        private T p;

        public event PropertyChangedEventHandler PropertyChanged;

        public T P
        {
            get => this.p;
            set
            {
                if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(value, this.p))
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(nameof(this.P));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void InsertCreatedFieldSorted()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p1;
        private int p3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1
        {
            get => this.p1;
            set
            {
                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        public int ↓P2 { get; set; }

        public int P3
        {
            get => this.p3;
            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged(nameof(this.P3));
            }
        }

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
        private int p1;
        private int p2;
        private int p3;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1
        {
            get => this.p1;
            set
            {
                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        public int P2
        {
            get => this.p2;
            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        public int P3
        {
            get => this.p3;
            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.OnPropertyChanged(nameof(this.P3));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void WhenFieldExists()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get; set; }

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
        private int p;
        private int p_;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.p_;
            set
            {
                if (value == this.p_)
                {
                    return;
                }

                this.p_ = value;
                this.OnPropertyChanged(nameof(this.P));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void InsertsFieldAfterPreviousBackingField()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p1;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1
        {
            get => this.p1;
            set
            {
                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged();
            }
        }

        public int ↓P2 { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p1;
        private int p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1
        {
            get => this.p1;
            set
            {
                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged();
            }
        }

        public int P2
        {
            get => this.p2;
            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void InsertsFieldBeforeNextBackingField()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P1 { get; set; }

        public int P2
        {
            get => this.p2;
            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p1;
        private int p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1
        {
            get => this.p1;
            set
            {
                if (value == this.p1)
                {
                    return;
                }

                this.p1 = value;
                this.OnPropertyChanged();
            }
        }

        public int P2
        {
            get => this.p2;
            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void HandlesFieldNamedAsKeyword()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string ↓Operator { get; set; }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
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
        private string @operator;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Operator
        {
            get => this.@operator;
            set
            {
                if (value == this.@operator)
                {
                    return;
                }

                this.@operator = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }
        }
    }
}
