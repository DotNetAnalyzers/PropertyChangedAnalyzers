namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        internal class ViewModelBase
        {
            private const string ViewModelBaseCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
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
            public void SetProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : ViewModelBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetValue(ref this.name, value) }
        }
    }
}";
                AnalyzerAssert.Valid<INPC003NotifyWhenPropertyChanges>(ViewModelBaseCode, testCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyNameOf()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetValue(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.Valid<INPC003NotifyWhenPropertyChanges>(ViewModelBaseCode, testCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyExpression()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : ViewModelBase
    {
        private int name;

        public string Greeting => $""Hello{this.Name}"";

        public int Name
        {
            get { return this.name; }
            set
            {
                if (this.SetValue(ref this.name, value))
                {
                    this.OnPropertyChanged(() => this.Greeting);
                }
            }
        }
    }
}";
                AnalyzerAssert.Valid<INPC003NotifyWhenPropertyChanges>(ViewModelBaseCode, testCode);
            }

            [Test]
            public void WhenOverriddenSet()
            {
                var fooBaseCode = @"
namespace RoslynSandbox
{
    public abstract class FooBase : ViewModelBase
    {
        public override bool Set<T>(ref T oldValue, T newValue, string propertyName = null)
        {
            return base.SetValue(ref oldValue, newValue, propertyName);
        }
    }
}";

                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private int value;

        public int Value
        {
            get { return this.value; }
            set { this.SetValue(ref this.value, value); }
        }
    }
}";

                AnalyzerAssert.Valid<INPC003NotifyWhenPropertyChanges>(fooBaseCode, testCode);
            }
        }
    }
}