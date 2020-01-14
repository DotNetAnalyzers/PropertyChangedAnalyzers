namespace PropertyChangedAnalyzers.Test.INPC016NotifyAfterUpdateTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetAccessorAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC016NotifyAfterMutation;

        private const string ViewModelBaseCode = @"
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

        [Test]
        public static void OnPropertyChangedAfterAssign()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.OnPropertyChanged(nameof(Value));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void PropertyChangedInvokeAfterAssign()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int value;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("this.TrySet(ref this.name, value);")]
        [TestCase("_ = this.TrySet(ref this.name, value);")]
        public static void AfterTrySet(string trySet)
        {
            var code = @"
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
                this.TrySet(ref this.name, value);
                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }
    }
}".AssertReplace("this.TrySet(ref this.name, value);", trySet);

            RoslynAssert.Valid(Analyzer, Descriptor, ViewModelBaseCode, code);
        }

        [Test]
        public static void AfterIfTrySetReturn()
        {
            var code = @"
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
                    return;
                }

                this.OnPropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, Descriptor, ViewModelBaseCode, code);
        }

        [Test]
        public static void InsideIfTrySetStatement()
        {
            var code = @"
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
                    this.OnPropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, ViewModelBaseCode, code);
        }

        [Test]
        public static void InsideIfTrySetBlock()
        {
            var code = @"
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
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, ViewModelBaseCode, code);
        }
    }
}
