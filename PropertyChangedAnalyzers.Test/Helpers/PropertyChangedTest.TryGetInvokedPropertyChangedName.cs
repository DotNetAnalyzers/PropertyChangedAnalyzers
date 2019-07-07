namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public partial class PropertyChangedTest
    {
        public static class TryGetInvokedPropertyChangedName
        {
            [TestCase("this.OnPropertyChanged()")]
            [TestCase("this.OnPropertyChanged(\"Bar\")")]
            [TestCase("this.OnPropertyChanged(nameof(Bar))")]
            [TestCase("this.OnPropertyChanged(nameof(this.Bar))")]
            [TestCase("this.OnPropertyChanged(() => Bar)")]
            [TestCase("this.OnPropertyChanged(() => this.Bar)")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(\"Bar\"))")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)))")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Bar)))")]
            [TestCase("this.OnPropertyChanged(Cached)")]
            [TestCase("this.OnPropertyChanged(args)")]
            public static void WhenTrue(string call)
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
                var args = new PropertyChangedEventArgs(""Bar"");
                this.OnPropertyChanged(args);
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
                var invocation = syntaxTree.FindInvocation(call);
                Assert.AreEqual(call, invocation.ToString());
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.TryGetName(invocation, semanticModel, CancellationToken.None, out var name));
                Assert.AreEqual("Bar", name);
            }

            [TestCase("this.OnPropertyChanged()")]
            [TestCase("this.OnPropertyChanged(\"Bar\")")]
            [TestCase("this.OnPropertyChanged(nameof(Bar))")]
            [TestCase("this.OnPropertyChanged(nameof(this.Bar))")]
            [TestCase("this.OnPropertyChanged(() => Bar)")]
            [TestCase("this.OnPropertyChanged(() => this.Bar)")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(\"Bar\"))")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)))")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Bar)))")]
            [TestCase("this.OnPropertyChanged(Cached)")]
            public static void WhenRecursive(string call)
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
                var invocation = syntaxTree.FindInvocation(call);
                Assert.AreEqual(call, invocation.ToString());
                Assert.AreEqual(AnalysisResult.No, PropertyChanged.TryGetName(invocation, semanticModel, CancellationToken.None, out _));
            }

            [TestCase("propertyName ?? string.Empty")]
            [TestCase("propertyName")]
            public static void WhenCachingInConcurrentDictionary(string expression)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> Cache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

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
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, Cache.GetOrAdd(propertyName ?? string.Empty, name => new PropertyChangedEventArgs(name)));
        }
    }
}".AssertReplace("propertyName ?? string.Empty", expression));
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("this.OnPropertyChanged();");
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.TryGetName(invocation, semanticModel, CancellationToken.None, out var name));
                Assert.AreEqual("Bar", name);
            }

            [TestCase("propertyName ?? string.Empty")]
            [TestCase("propertyName")]
            public static void WhenCachingInConcurrentDictionaryTempLocal(string expression)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace RoslynSandbox
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> Cache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

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
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var e = Cache.GetOrAdd(propertyName ?? string.Empty, name => new PropertyChangedEventArgs(name));
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace("propertyName ?? string.Empty", expression));
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var invocation = syntaxTree.FindInvocation("this.OnPropertyChanged();");
                Assert.AreEqual(AnalysisResult.Yes, PropertyChanged.TryGetName(invocation, semanticModel, CancellationToken.None, out var name));
                Assert.AreEqual("Bar", name);
            }
        }
    }
}
