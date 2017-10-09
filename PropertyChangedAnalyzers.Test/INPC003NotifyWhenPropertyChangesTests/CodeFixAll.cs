﻿namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFixAll
    {
        [Test]
        public void WhenUsingPropertiesExpressionBody()
        {
            var testCode = @"
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

            ↓this.firstName = value;
            this.OnPropertyChanged();
        }
    }

    public string LastName
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

            ↓this.lastName = value;
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var fixedCode = @"
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

    public string LastName
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

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            AnalyzerAssert.FixAll<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void WhenTwoCalculatedProperties()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                ↓this.name = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Greeting1));
                this.OnPropertyChanged(nameof(this.Greeting2));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.FixAll<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(testCode, fixedCode);
        }
    }
}