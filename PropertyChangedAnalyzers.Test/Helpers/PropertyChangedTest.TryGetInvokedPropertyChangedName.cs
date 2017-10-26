namespace PropertyChangedAnalyzers.Test
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    internal partial class PropertyChangedTest
    {
        internal class TryGetInvokedPropertyChangedName
        {
            [TestCase("this.OnPropertyChanged()")]
            [TestCase("this.OnPropertyChanged(\"Bar\"")]
            [TestCase("this.OnPropertyChanged(nameof(Bar)")]
            [TestCase("this.OnPropertyChanged(nameof(this.Bar)")]
            [TestCase("this.OnPropertyChanged(() => Bar")]
            [TestCase("this.OnPropertyChanged(() => this.Bar")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(\"Bar\")")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar))")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Bar))")]
            [TestCase("this.OnPropertyChanged(Cached)")]
            public void WhenTrue(string signature)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs Cached = new PropertyChangedEventArgs(""Bar"");

        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get
            {
                return this.bar;
            }

            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(""Bar"");
                this.OnPropertyChanged(nameof(Bar));
                this.OnPropertyChanged(nameof(this.Bar));
                this.OnPropertyChanged(() => Bar);
                this.OnPropertyChanged(() => this.Bar);
                this.OnPropertyChanged(new PropertyChangedEventArgs(""Bar""));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Bar)));
                this.OnPropertyChanged(Cached);
            }
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation(signature);
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.TryGetInvokedPropertyChangedName(invocation, semanticModel, CancellationToken.None, out _, out var name));
                Assert.AreEqual("Bar", name);
            }

            [TestCase("this.OnPropertyChanged()")]
            [TestCase("this.OnPropertyChanged(\"Bar\"")]
            [TestCase("this.OnPropertyChanged(nameof(Bar)")]
            [TestCase("this.OnPropertyChanged(nameof(this.Bar)")]
            [TestCase("this.OnPropertyChanged(() => Bar")]
            [TestCase("this.OnPropertyChanged(() => this.Bar")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(\"Bar\")")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar))")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Bar))")]
            [TestCase("this.OnPropertyChanged(Cached)")]
            public void WhenRecursive(string signature)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get
            {
                return this.bar;
            }

            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(""Bar"");
                this.OnPropertyChanged(nameof(Bar));
                this.OnPropertyChanged(nameof(this.Bar));
                this.OnPropertyChanged(() => Bar);
                this.OnPropertyChanged(() => this.Bar);
                this.OnPropertyChanged(new PropertyChangedEventArgs(""Bar""));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Bar)));
                this.OnPropertyChanged(Cached);
            }
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(property);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.OnPropertyChanged(propertyName);
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation(signature);
                Assert.AreEqual(AnalysisResult.No, PropertyChanged.TryGetInvokedPropertyChangedName(invocation, semanticModel, CancellationToken.None, out _, out var name));
            }
        }
    }
}