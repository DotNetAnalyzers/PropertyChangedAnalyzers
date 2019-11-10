namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentPropertyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class FixAll
    {
        private static readonly DiagnosticAnalyzer Analyzer = new MutationAnalyzer();
        private static readonly CodeFixProvider Fix = new NotifyForDependentPropertyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC003NotifyForDependentProperty);

        [Test]
        public static void WhenUsingPropertiesExpressionBody()
        {
            var before = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName => $""{this.FirstName} {this.LastName}"";

    public string FirstName
    {
        get => this.firstName;
        set
        {
            if (value == this.firstName)
            {
                return;
            }

            ↓this.firstName = value;
            this.OnPropertyChanged();
        }
    }

    public string LastName
    {
        get => this.lastName;
        set
        {
            if (value == this.lastName)
            {
                return;
            }

            ↓this.lastName = value;
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var after = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName => $""{this.FirstName} {this.LastName}"";

    public string FirstName
    {
        get => this.firstName;
        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    public string LastName
    {
        get => this.lastName;
        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenTwoCalculatedProperties()
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

        public int P1 => this.p;

        public int P2 => this.p * this.p;

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

                ↓this.p = value;
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
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

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
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
        
        [Test]
        public static void SimpleLambda()
        {
            var before = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public C()
        {
            Action<int> func = x => ↓this.p++;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var after = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public C()
        {
            Action<int> func = x =>
            {
                this.p++;
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void ParenthesizedLambda()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public C()
        {
            this.PropertyChanged += (o, e) => ↓this.p++;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

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

        public C()
        {
            this.PropertyChanged += (o, e) =>
            {
                this.p++;
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.p;

        public int P2 => this.p * this.p;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AddOneAfterOtherFieldAssignment()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p3;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.f * this.p3;

        public int P2 => this.f + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                ↓this.p3 = value;
                ↓this.f = value * 2;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
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
        private int p3;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.f * this.p3;

        public int P2 => this.f + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.f = value * 2;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AddTwoAfterOtherFieldAssignment()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p3;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.f * this.p3;

        public int P2 => this.f + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                ↓this.p3 = value;
                ↓this.f = value * 2;
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
        private int p3;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.f * this.p3;

        public int P2 => this.f + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.f = value * 2;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AddOneAfterOtherFieldAssignmentBeforeExplicitReturn()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p3;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.f * this.p3;

        public int P2 => this.f + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                ↓this.p3 = value;
                ↓this.f = value * 2;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                return;
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
        private int p3;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.f * this.p3;

        public int P2 => this.f + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.f = value * 2;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
                return;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void AddTwoAfterOtherFieldAssignmentBeforeExplicitReturn()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p3;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.f * this.p3;

        public int P2 => this.f + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                ↓this.p3 = value;
                ↓this.f = value * 2;
                this.OnPropertyChanged();
                return;
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
        private int p3;
        private int f;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P1 => this.f * this.p3;

        public int P2 => this.f + this.p3;

        public int P3
        {
            get
            {
                return this.p3;
            }

            set
            {
                if (value == this.p3)
                {
                    return;
                }

                this.p3 = value;
                this.f = value * 2;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
                this.OnPropertyChanged(nameof(this.P2));
                return;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
