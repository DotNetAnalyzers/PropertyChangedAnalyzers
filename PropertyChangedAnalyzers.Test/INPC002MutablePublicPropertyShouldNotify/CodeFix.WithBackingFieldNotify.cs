namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotify
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class WithBackingFieldNotify
        {
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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get
            {
                return this.p;
            }

            private set
            {
                this.p = value;
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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

            private set
            {
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int ↓P
        {
            get
            {
                return this.p;
            }

            private set
            {
                this.p = value;
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

    internal class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        internal int P
        {
            get
            {
                return this.p;
            }

            private set
            {
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get
            {
                return this.p;
            }
            private set
            {
                this.p = value;
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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }
            private set
            {
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Some text
        /// </summary>
        public int ↓P
        {
            /*1*/
            get
            {
                /*2*/ return this.p; /*3*/
            } /*4*/
            /*5*/
            set /*6*/
            {
                /*7*/ this.p = value; /*8*/
            } /*9*/
        } /*10*/

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
            /*1*/
            get
            {
                /*2*/
                return this.p; /*3*/
            } /*4*/
            /*5*/
            set /*6*/
            {
                /*7*/
                this.p = value; /*8*/
                this.OnPropertyChanged();
            } /*9*/
        } /*10*/

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
            }

            [Test]
            public static void PrivateSetSingeLine()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P { get { return this.p; } private set { this.p = value; }}

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

        public int P { get { return this.p; } private set { this.p = value; this.OnPropertyChanged(); } }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
            }

            [Test]
            public static void Private()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get { return this.p; }
            private set { this.p = value; }
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
            get { return this.p; }
            private set
            {
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual int ↓P
        {
            get { return this.p; }
            set { this.p = value; }
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

        public virtual int P
        {
            get { return this.p; }
            set
            {
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
            }

            [Test]
            public static void WithNestedBackingField()
            {
                var c1 = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C1 : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

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

                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get { return this.c1.P; }
            private set { this.c1.P = value; }
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

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 c1 = new C1();

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.c1.P; }
            private set
            {
                this.c1.P = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after, fixTitle: "Notify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after, fixTitle: "Notify.");
            }

            [Test]
            public static void WithNestedBackingFieldUnderscore()
            {
                var c1 = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C1 : INotifyPropertyChanged
    {
        private int _p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return _p;
            }

            set
            {
                if (value == _p)
                {
                    return;
                }

                _p = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 _c1 = new C1();

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get { return _c1.P; }
            private set { _c1.P = value; }
        }

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

    public class C2 : INotifyPropertyChanged
    {
        private readonly C1 _c1 = new C1();

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return _c1.P; }
            private set
            {
                _c1.P = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after, fixTitle: "Notify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { c1, before }, after, fixTitle: "Notify.");
            }

            [Test]
            public static void CallerMemberNameAccessorsOnOneLine()
            {
                var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ↓P
        {
            get { return this.p; }
            private set { this.p = value; }
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
            get { return this.p; }
            private set
            {
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
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Notify.");
            }
        }
    }
}
