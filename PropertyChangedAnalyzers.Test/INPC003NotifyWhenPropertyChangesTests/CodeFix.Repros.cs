namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        [Test]
        public void Vanguard_MVVM_ViewModels_MainWindowViewModel()
        {
            var childDataContext = @"
namespace Vanguard_MVVM.Infrastructure
{
    public interface IChildDataContext
    {
        string Title { get; }
    }
}";
            var testCode = @"
namespace Vanguard_MVVM.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Infrastructure;


    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        IChildDataContext _childDataContext;
        readonly string _title;
        MainWindowViewModel()
        {
            _title = ""MVVM Attempt"";
        }

        public IChildDataContext ChildDataContext
        {
            get { return _childDataContext; }

            private set
            {
                if (Equals(value, _childDataContext)) return;
                ↓_childDataContext = value;
                NotifyPropertyChanged(nameof(ChildDataContext));
            }
        }

        public static MainWindowViewModel Instance { get; } = new MainWindowViewModel();

        public string Title => ChildDataContext?.Title == null ? _title : string.Concat(_title, "" - "", ChildDataContext?.Title);


        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}";

            var fixedCode = @"
namespace Vanguard_MVVM.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Infrastructure;


    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        IChildDataContext _childDataContext;
        readonly string _title;
        MainWindowViewModel()
        {
            _title = ""MVVM Attempt"";
        }

        public IChildDataContext ChildDataContext
        {
            get { return _childDataContext; }

            private set
            {
                if (Equals(value, _childDataContext)) return;
                _childDataContext = value;
                NotifyPropertyChanged(nameof(ChildDataContext));
                NotifyPropertyChanged(nameof(Title));
            }
        }

        public static MainWindowViewModel Instance { get; } = new MainWindowViewModel();

        public string Title => ChildDataContext?.Title == null ? _title : string.Concat(_title, "" - "", ChildDataContext?.Title);


        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}";

            AnalyzerAssert.CodeFix<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(new[] { childDataContext, testCode }, fixedCode);
            AnalyzerAssert.FixAll<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(new[] { childDataContext, testCode }, fixedCode);
        }

        [Test]
        public void Vanguard_MVVM_ViewModels_MainWindowViewModelCommentedOut()
        {
            var childDataContext = @"namespace Vanguard_MVVM.Infrastructure
{
    public interface IChildDataContext
    {
        string Title { get; }
    }
}";
            var testCode = @"
namespace Vanguard_MVVM.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Infrastructure;


    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        IChildDataContext _childDataContext;
        readonly string _title;
        MainWindowViewModel()
        {
            _title = ""MVVM Attempt"";
        }

        public IChildDataContext ChildDataContext
        {
            get { return _childDataContext; }

            private set
            {
                if (Equals(value, _childDataContext)) return;
                ↓_childDataContext = value;
                NotifyPropertyChanged(nameof(ChildDataContext));
                //NotifyPropertyChanged(nameof(Title));
            }
        }

        public static MainWindowViewModel Instance { get; } = new MainWindowViewModel();

        public string Title => ChildDataContext?.Title == null ? _title : string.Concat(_title, "" - "", ChildDataContext?.Title);


        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}";

            var fixedCode = @"
namespace Vanguard_MVVM.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Infrastructure;


    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        IChildDataContext _childDataContext;
        readonly string _title;
        MainWindowViewModel()
        {
            _title = ""MVVM Attempt"";
        }

        public IChildDataContext ChildDataContext
        {
            get { return _childDataContext; }

            private set
            {
                if (Equals(value, _childDataContext)) return;
                _childDataContext = value;
                NotifyPropertyChanged(nameof(ChildDataContext));
                NotifyPropertyChanged(nameof(Title));
                //NotifyPropertyChanged(nameof(Title));
            }
        }

        public static MainWindowViewModel Instance { get; } = new MainWindowViewModel();

        public string Title => ChildDataContext?.Title == null ? _title : string.Concat(_title, "" - "", ChildDataContext?.Title);


        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}";

            AnalyzerAssert.CodeFix<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(new[] { childDataContext, testCode }, fixedCode);
            AnalyzerAssert.FixAll<INPC003NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>(new[] { childDataContext, testCode }, fixedCode);
        }
    }
}