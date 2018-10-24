namespace PropertyChangedAnalyzers.Test.INPC004UseCallerMemberNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [TestFixture(typeof(ArgumentAnalyzer))]
    [TestFixture(typeof(MethodDeclarationAnalyzer))]
    internal class ValidCode<T>
        where T : DiagnosticAnalyzer, new()
    {
        private static readonly T Analyzer = new T();

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void CallsRaisePropertyChangedWithEventArgs(string propertyName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
#pragma warning disable INPC013
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
#pragma warning restore INPC013
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void CallsRaisePropertyChangedCallerMemberName()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""""")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public void Invokes(string propertyName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
#pragma warning disable INPC013
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar))));
#pragma warning restore INPC013
            }
        }
    }
}";
            testCode = testCode.AssertReplace(@"nameof(this.Bar))", propertyName);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void InvokesCached()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs BarPropertyChangedArgs = new PropertyChangedEventArgs(nameof(Bar));
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, BarPropertyChangedArgs);
            }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void UpdateMethod()
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoreWhenRaiseForOtherInstance()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
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
            var vm = new ViewModel();
            vm.OnPropertyChanged(propertyName);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoreWhenRaiseForOtherInstanceOfOtherType()
        {
            var viewModelCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
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

            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public void RaiseForChild(string propertyName)
        {
            var vm = new ViewModel();
            vm.OnPropertyChanged(propertyName);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, viewModelCode, testCode);
        }

        [Test]
        public void IgnoreWhenCallingFrameworkBaseClass()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void TimeSpanTicks()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void ExceptionHandlingRelayCommand()
        {
            var testCode = @"
namespace RoslynSandbox
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
