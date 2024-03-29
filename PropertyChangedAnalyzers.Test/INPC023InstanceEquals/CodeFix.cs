﻿namespace PropertyChangedAnalyzers.Test.INPC023InstanceEquals;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly SetAccessorAnalyzer Analyzer = new();
    private static readonly EqualityFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC023InstanceEquals);

    [TestCase("int?", "value == this.p")]
    [TestCase("string", "value == this.p")]
    [TestCase("string?", "value == this.p")]
    public static void WhenNullable(string type, string expected)
    {
        var before = @"
#pragma warning disable CS8618
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int? P
        {
            get => this.p;
            set
            {
                if (↓value.Equals(this.p))
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
}".AssertReplace("int?", type);

        var after = @"
#pragma warning disable CS8618
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int? P
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("int?", type)
.AssertReplace("value == this.p", expected);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("int?", "value != this.p")]
    [TestCase("string", "value != this.p")]
    [TestCase("string?", "value != this.p")]
    public static void Negated(string type, string expected)
    {
        var before = @"
#pragma warning disable CS8618
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int? P
        {
            get => this.p;
            set
            {
                if (!↓value.Equals(this.p))
                {
                    this.p = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("int?", type);

        var after = @"
#pragma warning disable CS8618
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int? p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int? P
        {
            get => this.p;
            set
            {
                if (value != this.p)
                {
                    this.p = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}".AssertReplace("int?", type)
.AssertReplace("value != this.p", expected);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
