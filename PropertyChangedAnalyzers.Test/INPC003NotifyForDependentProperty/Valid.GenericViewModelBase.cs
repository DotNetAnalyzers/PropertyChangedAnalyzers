﻿namespace PropertyChangedAnalyzers.Test.INPC003NotifyForDependentProperty;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class Valid
{
    public static class GenericViewModelBase
    {
        private const string ViewModelBaseOfT = @"
namespace N.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual bool TrySet<TValue>(ref TValue field, TValue value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<TValue>(Expression<Func<TValue>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        [Test]
        public static void SetProperty()
        {
            var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase<int>
    {
        private string? p;

        public string? P
        {
            get { return this.p; }
            set { this.TrySet(ref this.p, value); }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseOfT, code);
        }

        [Test]
        public static void SetPropertyWhenNullCoalescingInTrySet()
        {
            var viewModelBase = @"
namespace N.Core
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName ?? string.Empty);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase
    {
        private string? p;

        public string? P
        {
            get { return this.p; }
            set { this.TrySet(ref this.p, value); }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, viewModelBase, code);
        }

        [Test]
        public static void SetPropertyExpressionBodies()
        {
            var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase<int>
    {
        private string? p;

        public string? P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseOfT, code);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyExplicitNameOf()
        {
            var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase<int>
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value, nameof(P2)))
                {
                    this.OnPropertyChanged(nameof(P1));
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseOfT, code);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyNameOf()
        {
            var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase<int>
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(P1));
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseOfT, code);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyStringEmpty()
        {
            var code = @"
namespace N
{
    public class C : N.Core.ViewModelBase<int>
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get => this.p2;
            set => this.TrySet(ref this.p2, value, string.Empty);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseOfT, code);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyExpression()
        {
            var code = @"
namespace N.Client
{
    public class C : N.Core.ViewModelBase<int>
    {
        private string? p2;

        public string P1 => $""Hello{this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(() => this.P1);
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseOfT, code);
        }

        [Test]
        public static void WhenOverriddenSet()
        {
            var viewModelBase = @"
namespace N.Client
{
    public abstract class ViewModelBase : N.Core.ViewModelBase<int>
    {
        protected override bool TrySet<T>(ref T oldValue, T value, string? propertyName = null)
        {
            return base.TrySet(ref oldValue, value, propertyName);
        }
    }
}";

            var code = @"
namespace N.Client
{
    public class C : ViewModelBase
    {
        private int p;

        public int P
        {
            get { return this.p; }
            set { this.TrySet(ref this.p, value); }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, ViewModelBaseOfT, viewModelBase, code);
        }
    }
}
