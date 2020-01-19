namespace PropertyChangedAnalyzers.Test.INPC020PreferExpressionBodyAccessor
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class FixAll
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new ExpressionBodyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC020PreferExpressionBodyAccessor);

        [Test]
        public static void DependencyProperty()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class WpfControl : Control
    {
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int),
            typeof(WpfControl),
            new PropertyMetadata(default(int)));

        public int Number
        {
            ↓get { return (int)this.GetValue(NumberProperty); }
            ↓set { this.SetValue(NumberProperty, value); }
        }
    }
}";
            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class WpfControl : Control
    {
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int),
            typeof(WpfControl),
            new PropertyMetadata(default(int)));

        public int Number
        {
            get => (int)this.GetValue(NumberProperty);
            set => this.SetValue(NumberProperty, value);
        }
    }
}";

            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
