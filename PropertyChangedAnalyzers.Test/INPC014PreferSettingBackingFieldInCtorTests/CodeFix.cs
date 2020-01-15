namespace PropertyChangedAnalyzers.Test.INPC014PreferSettingBackingFieldInCtorTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AssignmentAnalyzer();
        private static readonly CodeFixProvider Fix = new SetBackingFieldFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC014SetBackingFieldInConstructor);

        private const string ViewModelBase = @"
namespace N.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        [Test]
        public static void SimplePropertyWithBackingFieldStatementBodySetter()
        {
            var before = @"
namespace N
{
    public class C
    {
        private int p;

        public C(int p)
        {
            ↓this.P = p;
        }

        public int P
        {
            get => this.p;
            private set
            {
                this.p = value;
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        private int p;

        public C(int p)
        {
            this.p = p;
        }

        public int P
        {
            get => this.p;
            private set
            {
                this.p = value;
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void SimplePropertyWithBackingFieldExpressionBodySetter()
        {
            var before = @"
namespace N
{
    public class C
    {
        private int p;

        public C(int p)
        {
            ↓this.P = p;
        }

        public int P
        {
            get => this.p;
            private set => this.p = value;
        }
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        private int p;

        public C(int p)
        {
            this.p = p;
        }

        public int P
        {
            get => this.p;
            private set => this.p = value;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void SimplePropertyWithBackingFieldExpressionBodySetterKeyword()
        {
            var before = @"
namespace N
{
    public class C
    {
        private int @default;

        public C(int @default)
        {
            ↓this.P = @default;
        }

        public int P
        {
            get => this.@default;
            private set => this.@default = value;
        }
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        private int @default;

        public C(int @default)
        {
            this.@default = @default;
        }

        public int P
        {
            get => this.@default;
            private set => this.@default = value;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void SimplePropertyWithBackingFieldExpressionBodySetterCollisionParameter()
        {
            var before = @"
namespace N
{
    public class C
    {
        private int f;

        public C(int f)
        {
            ↓this.P = f;
        }

        public int P
        {
            get => f;
            private set => f = value;
        }
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        private int f;

        public C(int f)
        {
            this.f = f;
        }

        public int P
        {
            get => f;
            private set => f = value;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void SimplePropertyWithBackingFieldExpressionBodySetterCollisionLocal()
        {
            var before = @"
namespace N
{
    public class C
    {
        private int p;

        public C(int value)
        {
            var p = 1;
            ↓this.P = value;
        }

        public int P
        {
            get => p;
            private set => p = value;
        }
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        private int p;

        public C(int value)
        {
            var p = 1;
            this.p = value;
        }

        public int P
        {
            get => p;
            private set => p = value;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void SimplePropertyWithBackingFieldUnderscoreNames()
        {
            var before = @"
namespace N
{
    public class C
    {
        private int _p;

        public C(int p)
        {
            ↓P = p;
        }

        public int P
        {
            get => _p;
            private set
            {
                _p = value;
            }
        }
    }
}";

            var after = @"
namespace N
{
    public class C
    {
        private int _p;

        public C(int p)
        {
            _p = p;
        }

        public int P
        {
            get => _p;
            private set
            {
                _p = value;
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("value == this.p")]
        [TestCase("this.p == value")]
        [TestCase("Equals(this.p, value)")]
        [TestCase("Equals(value, this.p)")]
        [TestCase("ReferenceEquals(this.p, value)")]
        [TestCase("ReferenceEquals(value, this.p)")]
        [TestCase("value.Equals(this.p)")]
        [TestCase("this.p.Equals(value)")]
        public static void NotifyingProperty(string equals)
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C(string p)
        {
            ↓this.P = p;
        }

        [DataMember]
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
}".AssertReplace("value == this.p", equals);

            var after = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class C : INotifyPropertyChanged
    {
        private string p;

        public event PropertyChangedEventHandler PropertyChanged;

        public C(string p)
        {
            this.p = p;
        }

        [DataMember]
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
}".AssertReplace("value == this.p", equals);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenSettingFieldUsingTrySet()
        {
            var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public C(int p)
        {
            ↓this.P = p;
        }
        
        public int P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }
    }
}";
            var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private int p;

        public C(int p)
        {
            this.p = p;
        }
        
        public int P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBase, before }, after);
        }

        [Test]
        public static void WhenSettingFieldUsingTrySetAndNotifyForOther()
        {
            var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public C(string p2)
        {
            ↓this.P2 = p2;
        }
        
        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
            var after = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string p2;

        public C(string p2)
        {
            this.p2 = p2;
        }
        
        public string P1 => $""Hello {this.P2}"";

        public string P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBase, before }, after);
        }

        [Test]
        public static void WhenShadowingParameter()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public C(bool x)
        {
            ↓X = x;
        }

        private bool x;

        public bool X
        {
            get => x;
            set
            {
                if (value == x)
                {
                    return;
                }

                x = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
        public C(bool x)
        {
            this.x = x;
        }

        private bool x;

        public bool X
        {
            get => x;
            set
            {
                if (value == x)
                {
                    return;
                }

                x = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBase, before }, after);
        }

        [Test]
        public static void WhenShadowingLocal()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public C(bool a)
        {
            var x = a;
            ↓X = a;
        }

        private bool x;

        public bool X
        {
            get => x;
            set
            {
                if (value == x)
                {
                    return;
                }

                x = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
        public C(bool a)
        {
            var x = a;
            this.x = a;
        }

        private bool x;

        public bool X
        {
            get => x;
            set
            {
                if (value == x)
                {
                    return;
                }

                x = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBase, before }, after);
        }

        [Test]
        public static void WhenShadowingLocalKeyword()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        public C(bool a)
        {
            var @default = a;
            ↓X = a;
        }

        private bool @default;

        public bool X
        {
            get => @default;
            set
            {
                if (value == @default)
                {
                    return;
                }

                @default = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
        public C(bool a)
        {
            var @default = a;
            this.@default = a;
        }

        private bool @default;

        public bool X
        {
            get => @default;
            set
            {
                if (value == @default)
                {
                    return;
                }

                @default = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ViewModelBase, before }, after);
        }
    }
}
