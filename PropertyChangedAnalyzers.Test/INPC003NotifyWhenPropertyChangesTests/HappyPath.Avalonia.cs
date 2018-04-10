namespace PropertyChangedAnalyzers.Test.INPC003NotifyWhenPropertyChangesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal partial class HappyPath
    {
        internal class AvaloniaHappyPath
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.AddTransitiveMetadataReferences(typeof(Avalonia.AvaloniaObject).Assembly);
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
            public void SetAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using Avalonia
    public class ViewModel : AvaloniaObject
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if(name==value)
                {
                    return;
                }
                string previousGreeting=Greeting;
                this.SetAndRaise(NameProperty, ref name, value);
                this.RaisePropertyChange(GreetingProperty, previousGreeting, Greeting);
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
