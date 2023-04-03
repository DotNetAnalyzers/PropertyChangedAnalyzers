namespace PropertyChangedAnalyzers.Test.INPC019GetBackingField;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

public static class Valid
{
    private static readonly PropertyDeclarationAnalyzer Analyzer = new();
    private static readonly DiagnosticDescriptor Descriptor = Descriptors.INPC019GetBackingField;

    [Test]
    public static void ExpressionBody()
    {
        var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ExpressionBodies : INotifyPropertyChanged
    {
        private string? p2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string? P2
        {
            get => this.p2;

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void TrySetExpressionBody()
    {
        var code = @"
namespace ValidCode.TrySet
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ExpressionBodies : INotifyPropertyChanged
    {
        private string? p2;
        private int p;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string? P2
        {
            get => this.p2;

            set
            {
                if (this.TrySet(ref this.p2, value))
                {
                    this.OnPropertyChanged(nameof(this.P1));
                }
            }
        }

        public int P
        {
            get => this.p;
            set => this.TrySet(ref this.p, value);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool TrySet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void StatementBody()
    {
        var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class ExpressionBodies : INotifyPropertyChanged
    {
        private string? p2;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string? P2
        {
            get
            {
                return this.p2;
            }

            set
            {
                if (value == this.p2)
                {
                    return;
                }

                this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [Test]
    public static void DependencyProperty()
    {
        var code = @"
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
            get { return (int) this.GetValue(NumberProperty); }
            set { this.SetValue(NumberProperty, value); }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }
}
