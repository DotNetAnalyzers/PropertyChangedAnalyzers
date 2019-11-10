namespace PropertyChangedAnalyzers.Test.INPC004UseCallerMemberNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [TestFixture(typeof(ArgumentAnalyzer))]
    [TestFixture(typeof(MethodDeclarationAnalyzer))]
    public static class Valid<T>
        where T : DiagnosticAnalyzer, new()
    {
        private static readonly T Analyzer = new T();

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""P""")]
        [TestCase(@"nameof(P)")]
        [TestCase(@"nameof(this.P)")]
        public static void CallsRaisePropertyChangedWithEventArgs(string propertyName)
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
#pragma warning disable INPC013
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
#pragma warning restore INPC013
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace(@"nameof(P)", propertyName);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void CallsRaisePropertyChangedCallerMemberName()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p) return;
                this.p = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""P""")]
        [TestCase(@"nameof(P)")]
        [TestCase(@"nameof(this.P)")]
        public static void Invokes(string propertyName)
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
#pragma warning disable INPC013
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.P))));
#pragma warning restore INPC013
            }
        }
    }
}".AssertReplace(@"nameof(this.P))", propertyName);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void InvokesCached()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;

    public class C : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PPropertyChangedArgs = new PropertyChangedEventArgs(nameof(P));
        private int p;
        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get { return this.p; }
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.PropertyChanged?.Invoke(this, PPropertyChangedArgs);
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void UpdateMethod()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private string text;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                if (value == this.text)
                {
                    return;
                }

                this.text = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void Update(string newText)
        {
            this.text = newText;
            this.OnPropertyChanged(nameof(this.Text));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoreWhenRaiseForOtherInstance()
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
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaiseForChild(string propertyName)
        {
            var vm = new C();
            vm.OnPropertyChanged(propertyName);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void IgnoreWhenRaiseForOtherInstanceOfOtherType()
        {
            var c1 = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C1 : INotifyPropertyChanged
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
                this.OnPropertyChanged();
            }
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var code = @"
namespace N
{
    public class C
    {
        public void RaiseForChild(string propertyName)
        {
            var vm = new C1();
            vm.OnPropertyChanged(propertyName);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, c1, code);
        }

        [Test]
        public static void IgnoreWhenCallingFrameworkBaseClass()
        {
            var code = @"
namespace N
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    internal class CellTemplateColumn : DataGridTemplateColumn
    {
        private BindingBase binding;

        public BindingBase Binding
        {
            get
            {
                return this.binding;
            }

            set
            {
                if (this.binding != value)
                {
                    this.binding = value;
                    this.CoerceValue(DataGridColumn.SortMemberPathProperty);
                    this.NotifyPropertyChanged(nameof(this.Binding));
                }
            }
        }

        public override BindingBase ClipboardContentBinding
        {
            get { return base.ClipboardContentBinding ?? this.Binding; }
            set { base.ClipboardContentBinding = value; }
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return this.LoadTemplateContent(true, dataItem, cell);
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            return this.LoadTemplateContent(false, dataItem, cell);
        }

        private DataTemplate ChooseCellTemplate(bool isEditing)
        {
            DataTemplate template = null;
            if (isEditing)
            {
                template = this.CellEditingTemplate;
            }

            if (template == null)
            {
                template = this.CellTemplate;
            }

            return template;
        }

        private DataTemplateSelector ChooseCellTemplateSelector(bool isEditing)
        {
            DataTemplateSelector templateSelector = null;
            if (isEditing)
            {
                templateSelector = this.CellEditingTemplateSelector;
            }

            if (templateSelector == null)
            {
                templateSelector = this.CellTemplateSelector;
            }

            return templateSelector;
        }

        [SuppressMessage(""ReSharper"", ""UnusedParameter.Local"")]
        private FrameworkElement LoadTemplateContent(bool isEditing, object dataItem, DataGridCell cell)
        {
            var template = this.ChooseCellTemplate(isEditing);
            var templateSelector = this.ChooseCellTemplateSelector(isEditing);
            if ((template == null) && (templateSelector == null))
            {
                return null;
            }

            var contentPresenter = new ContentPresenter();
            BindingOperations.SetBinding(contentPresenter, ContentPresenter.ContentProperty, this.binding);
            contentPresenter.ContentTemplate = template;
            contentPresenter.ContentTemplateSelector = templateSelector;
            return contentPresenter;
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void TimeSpanTicks()
        {
            var code = @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private TimeSpan timeSpan;

        public event PropertyChangedEventHandler PropertyChanged;

        public long Ticks
        {
            get => this.timeSpan.Ticks;
            set
            {
                if (value == this.timeSpan.Ticks)
                {
                    return;
                }

                this.timeSpan = TimeSpan.FromTicks(value);
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ExceptionHandlingRelayCommand()
        {
            var code = @"
namespace N
{
    using System;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;

    public class ExceptionHandlingRelayCommand : ConditionRelayCommand
    {
        private Exception _exception;

        public ExceptionHandlingRelayCommand(Action action, ICondition condition)
            : base(action, condition)
        {
        }

        public Exception Exception
        {
            get => _exception;

            private set
            {
                if (Equals(value, _exception))
                {
                    return;
                }

                _exception = value;
                OnPropertyChanged();
            }
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
