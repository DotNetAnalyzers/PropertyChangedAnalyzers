namespace PropertyChangedAnalyzers.Test.INPC021SetBackingFieldTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetAccessorAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.INPC021SetBackingField);

        [Test]
        public static void ExpressionBodyNotAssigning()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ExpressionBodies : INotifyPropertyChanged
    {
        private string p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get => this.p2;
            ↓set
            {
                if (value == this.p2)
                {
                    return;
                }

                // this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
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

    public class ExpressionBodies : INotifyPropertyChanged
    {
        private string p2;
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get => this.p2;

            ↓set
            {
                // if (this.TrySet(ref this.p2, value))
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool TrySet<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void StatementBody()
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ExpressionBodies : INotifyPropertyChanged
    {
        private string p2;

        public event PropertyChangedEventHandler PropertyChanged;

        public string P1 => $""Hello {this.p2}"";

        public string P2
        {
            get
            {
                return this.p2;
            }

            ↓set
            {
                if (value == this.p2)
                {
                    return;
                }

                // this.p2 = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.P1));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
