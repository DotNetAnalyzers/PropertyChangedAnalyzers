namespace PropertyChangedAnalyzers.Test.INPC004UseCallerMemberNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class FixAll
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ArgumentAnalyzer();
        private static readonly UseCallerMemberNameFix Fix = new UseCallerMemberNameFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC004UseCallerMemberName);

        [Test]
        public static void FixAllTest()
        {
            var before = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int value1;
        private int value2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Sum => this.value1 + this.value2;

        public int Value1
        {
            get
            {
                return this.value1;
            }

            set
            {
                if (value == this.value1)
                {
                    return;
                }

                this.value1 = value;
                this.OnPropertyChanged(↓nameof(Value1));
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

        public int Value2
        {
            get
            {
                return this.value2;
            }

            set
            {
                if (value == this.value2)
                {
                    return;
                }

                this.value2 = value;
                this.OnPropertyChanged(↓nameof(Value2));
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

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
        private int value1;
        private int value2;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Sum => this.value1 + this.value2;

        public int Value1
        {
            get
            {
                return this.value1;
            }

            set
            {
                if (value == this.value1)
                {
                    return;
                }

                this.value1 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

        public int Value2
        {
            get
            {
                return this.value2;
            }

            set
            {
                if (value == this.value2)
                {
                    return;
                }

                this.value2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Sum));
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyExplicitNameOf()
        {
            var viewModelBaseCode = @"
namespace N.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var before = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.TrySet(ref this.name, value, ↓nameof(Name)))
                {
                    this.OnPropertyChanged(nameof(Greeting));
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
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.TrySet(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(Greeting));
                }
            }
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { viewModelBaseCode, before }, after);
        }
    }
}
