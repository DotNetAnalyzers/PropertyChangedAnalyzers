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
            [TestCase("this.OnPropertyChanged(\"P\")")]
            [TestCase("this.OnPropertyChanged(nameof(P))")]
            [TestCase("this.OnPropertyChanged(nameof(this.P))")]
            [TestCase("this.OnPropertyChanged(() => P)")]
            [TestCase("this.OnPropertyChanged(() => this.P)")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(\"P\"))")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)))")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.P)))")]
            [TestCase("this.OnPropertyChanged(Cached)")]
            [TestCase("this.OnPropertyChanged(args)")]
            public static void WhenTrue(string call)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs Cached = new PropertyChangedEventArgs(""P"");

        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(""P"");
                this.OnPropertyChanged(nameof(P));
                this.OnPropertyChanged(nameof(this.P));
                this.OnPropertyChanged(() => P);
                this.OnPropertyChanged(() => this.P);
                this.OnPropertyChanged(new PropertyChangedEventArgs(""P""));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.P)));
                this.OnPropertyChanged(Cached);
                var args = new PropertyChangedEventArgs(""P"");
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
                var findPropertyName = PropertyChanged.FindPropertyName(invocation, semanticModel, CancellationToken.None);
                Assert.AreEqual("P", findPropertyName?.Name);
            }

            [TestCase("this.OnPropertyChanged()")]
            [TestCase("this.OnPropertyChanged(\"P\")")]
            [TestCase("this.OnPropertyChanged(nameof(P))")]
            [TestCase("this.OnPropertyChanged(nameof(this.P))")]
            [TestCase("this.OnPropertyChanged(() => P)")]
            [TestCase("this.OnPropertyChanged(() => this.P)")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(\"P\"))")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)))")]
            [TestCase("this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.P)))")]
            [TestCase("this.OnPropertyChanged(Cached)")]
            public static void WhenRecursive(string call)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(""P"");
                this.OnPropertyChanged(nameof(P));
                this.OnPropertyChanged(nameof(this.P));
                this.OnPropertyChanged(() => P);
                this.OnPropertyChanged(() => this.P);
                this.OnPropertyChanged(new PropertyChangedEventArgs(""P""));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(P)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.P)));
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
                Assert.AreEqual(null, PropertyChanged.FindPropertyName(invocation, semanticModel, CancellationToken.None));
            }

            [TestCase("propertyName ?? string.Empty")]
            [TestCase("propertyName")]
            public static void WhenCachingInConcurrentDictionary(string expression)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> Cache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
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
                var findPropertyName = PropertyChanged.FindPropertyName(invocation, semanticModel, CancellationToken.None);
                Assert.AreEqual("P", findPropertyName?.Name);
            }

            [TestCase("propertyName ?? string.Empty")]
            [TestCase("propertyName")]
            public static void WhenCachingInConcurrentDictionaryTempLocal(string expression)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    @"
namespace N
{
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> Cache = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get
            {
                return this.p;
            }

            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
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
                var findPropertyName = PropertyChanged.FindPropertyName(invocation, semanticModel, CancellationToken.None);
                Assert.AreEqual("P", findPropertyName?.Name);
            }
        }
    }
}
