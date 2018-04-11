namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    internal partial class HappyPath
    {
        internal class Avalonia
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.MetadataReferences.AddRange(SpecialMetadataReferences.AvaloniaReferences);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetMetadataReferences();
            }

            [Test]
            public void SetAndRaiseProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Avalonia.AvaloniaObject
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetAndRaise(NameProperty, ref name, value) }
        }

        public static readonly Avalonia.AvaloniaProperty<string> NameProperty = Avalonia.AvaloniaProperty.Register<ViewModel, string>(nameof(Name));
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetAndRaisePropertyExpressionBodies()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Avalonia.AvaloniaObject
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.SetAndRaise(NameProperty, ref this.name, value)
        }
        
        public static readonly Avalonia.AvaloniaProperty<string> NameProperty = Avalonia.AvaloniaProperty.Register<ViewModel,string>(nameof(Name));
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void RaisePropertyChanged()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : Avalonia.AvaloniaObject
    {
        private string name;

        public string Name
        {
            get => this.name;
            set {
                if(this.name==value)
                {
                    return;
                }
                this.RaisePropertyChanged(NameProperty, this.name, value);
                this.name=value;
            }
        }
        
        public static readonly Avalonia.AvaloniaProperty<string> NameProperty = Avalonia.AvaloniaProperty.Register<ViewModel,string>(nameof(Name));
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }

            [Test]
            public void SetAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using Avalonia;
    public class ViewModel : AvaloniaObject
    {
        private string name;
        
        public string Greeting { get { return $""Hello {this.name}""; } }

        public string Name
        {
            get { return this.name; }
            set
            {
                if(name==value)
                {
                    return;
                }
                string prevGreeting = this.Greeting;
                this.SetAndRaise(NameProperty, ref this.name, value))
                this.RaisePropertyChanged(GreetingProperty, prevGreeting, this.Greeting);
            }
        }

        public static readonly AvaloniaProperty<string> NameProperty = AvaloniaProperty.Register<ViewModel,string>(nameof(Name));
        public static readonly AvaloniaProperty<string> GreetingProperty = AvaloniaProperty.Register<ViewModel,string>(nameof(Greeting));
    }
}";
                AnalyzerAssert.Valid(Analyzer, testCode);
            }
        }
    }
}
