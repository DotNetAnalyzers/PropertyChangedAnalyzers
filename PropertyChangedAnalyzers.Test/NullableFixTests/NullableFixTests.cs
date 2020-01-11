namespace PropertyChangedAnalyzers.Test.NullableFixTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static class NullableFixTests
    {
        private static readonly CodeFixProvider Fix = new NullableFix();
        private static readonly ExpectedDiagnostic CS8618 = ExpectedDiagnostic.Create("CS8618");
        private static readonly ExpectedDiagnostic CS8625 = ExpectedDiagnostic.Create("CS8625", "Cannot convert null literal to non-nullable reference type.");
        private static readonly CSharpCompilationOptions CompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable);

        [Test]
        public static void DeclareEventNullable()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler ↓PropertyChanged;

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

            var after = @"
namespace N
{
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
}";
            RoslynAssert.CodeFix(Fix, CS8618, before, after, compilationOptions: CompilationOptions, fixTitle: "Declare PropertyChanged as nullable.");
        }

        [Test]
        public static void DeclareEventNullableWhenConstructor()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public ↓C(int p)
        {
            this.p = p;
        }

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

            var after = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public C(int p)
        {
            this.p = p;
        }

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
}";
            RoslynAssert.CodeFix(Fix, CS8618, before, after, compilationOptions: CompilationOptions);
        }

        [Test]
        public static void DeclareDefaultParameterNullable()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = ↓null)
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Fix, CS8625, before, after, compilationOptions: CompilationOptions, fixTitle: "Declare propertyName as nullable.");
        }

        [Test]
        public static void DeclareFieldAndPropertyNullable()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string ↓p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        private string? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? P
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.CodeFix(Fix, CS8618, before, after, compilationOptions: CompilationOptions, fixTitle: "Declare field p and property P as nullable.");
        }

        [Test]
        public static void OpenGenericFieldAndPropertyNullableNoFix()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C<T> : INotifyPropertyChanged
    {
        private T ↓p;

        public event PropertyChangedEventHandler? PropertyChanged;

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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.NoFix(Fix, CS8618, new[] { before }, compilationOptions: CompilationOptions);
        }
    }
}
