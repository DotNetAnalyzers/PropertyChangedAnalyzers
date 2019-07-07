namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        [Test]
        public static void Vanguard_MVVM_ViewModels_MainWindowViewModel()
        {
            var childDataContext = @"
namespace Vanguard_MVVM.ViewModels
{
    public interface IChildDataContext
    {
        string Title { get; }
    }
}";
            var before = @"
namespace Vanguard_MVVM.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

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

            var after = @"
namespace Vanguard_MVVM.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { childDataContext, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { childDataContext, before }, after);
        }

        [Test]
        public static void Vanguard_MVVM_ViewModels_MainWindowViewModelCommentedOut()
        {
            var childDataContext = @"namespace Vanguard_MVVM.ViewModels
{
    public interface IChildDataContext
    {
        string Title { get; }
    }
}";
            var before = @"
namespace Vanguard_MVVM.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

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

            var after = @"
namespace Vanguard_MVVM.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

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

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { childDataContext, before }, after);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { childDataContext, before }, after);
        }
    }
}
