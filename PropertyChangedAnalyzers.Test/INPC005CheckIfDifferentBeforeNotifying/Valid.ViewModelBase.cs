﻿namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifying;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class Valid
{
    public static class ViewModelBase
    {
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
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
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

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

        [Test]
        public static void SetAffectsCalculatedProperty()
        {
            var code = @"
namespace N
{
    public class C : N.Core.ViewModelBase
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
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseCode, code);
        }

        [Test]
        public static void SetAffectsCalculatedPropertyEmptyIf()
        {
            var code = @"
namespace N
{
    public class C : N.Core.ViewModelBase
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
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseCode, code);
        }

        [Test]
        public static void SetAffectsSecondCalculatedProperty()
        {
            var code = @"
namespace N
{
    public class C : N.Core.ViewModelBase
    {
        private string? p3;

        public string P1 => $""Hello {this.P3}"";

        public string P2 => $""Hej {this.P3}"";

        public string? P3
        {
            get { return this.p3; }
            set
            {
                if (this.TrySet(ref this.p3, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                    this.OnPropertyChanged(nameof(this.P2));
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseCode, code);
        }

        [Test]
        public static void SetAffectsSecondCalculatedPropertyMissingBraces()
        {
            var code = @"
namespace N
{
    public class C : N.Core.ViewModelBase
    {
        private string? p3;

        public string P1 => $""Hello {this.P3}"";

        public string P2 => $""Hej {this.P3}"";

        public string? P3
        {
            get { return this.p3; }
            set
            {
                if (this.TrySet(ref this.p3, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                    this.OnPropertyChanged(nameof(this.P2));
                }
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseCode, code);
        }

        [Test]
        public static void OnPropertyChangedAffectsCalculatedProperty()
        {
            var code = @"
namespace N
{
    public class C : N.Core.ViewModelBase
    {
        private string? firstName;
        private string? lastName;

        public string FullName => $""{this.FirstName} {this.LastName}"";

        public string? FirstName
        {
            get
            {
                return this.firstName;
            }

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

        public string? LastName
        {
            get
            {
                return this.lastName;
            }

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
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseCode, code);
        }

        [Test]
        public static void IfNotSetReturnCalculatedProperty()
        {
            var code = @"
namespace N
{
    public class C : N.Core.ViewModelBase
    {
        private string? p2;

        public string P1 => $""Hello {this.P2}"";

        public string? P2
        {
            get { return this.p2; }
            set
            {
                if (!TrySet(ref this.p2, value))
                {
                    return;
                }

                this.OnPropertyChanged(nameof(this.P1));
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, ViewModelBaseCode, code);
        }
    }
}
