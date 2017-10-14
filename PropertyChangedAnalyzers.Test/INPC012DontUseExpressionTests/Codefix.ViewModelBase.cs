namespace PropertyChangedAnalyzers.Test.INPC012DontUseExpressionTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class Codefix
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

        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
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
                if (this.Set(ref this.name, value))
                {
                    this.OnPropertyChanged(↓() => this.Greeting);
                }
            }
        }
    }
}";

                var fixedCode = @"
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
                if (this.Set(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC012DontUseExpression, RemoveExpressionCodeFix>(new[] { ViewModelBaseCode, testCode }, fixedCode);
            }
        }
    }
}