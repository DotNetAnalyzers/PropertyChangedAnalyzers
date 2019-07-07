namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static class PropertyChangedEventArgsTests
    {
        [TestCase("private static readonly PropertyChangedEventArgs Cached = new PropertyChangedEventArgs(\"Bar\");")]
        [TestCase("private static readonly PropertyChangedEventArgs Cached = new PropertyChangedEventArgs(nameof(Bar));")]
        [TestCase("public static PropertyChangedEventArgs Cached { get; } = new PropertyChangedEventArgs(nameof(Bar));")]
        public static void Cached(string cached)
        {
            var code = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs Cached = new PropertyChangedEventArgs(""Bar"");

        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get => this.bar;

            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(Cached);
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace("private static readonly PropertyChangedEventArgs Cached = new PropertyChangedEventArgs(\"Bar\");", cached);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var argument = syntaxTree.FindInvocation("this.OnPropertyChanged(Cached)").ArgumentList.Arguments[0];
            Assert.AreEqual(true, PropertyChangedEventArgs.TryGetPropertyName(argument.Expression, semanticModel, CancellationToken.None, out var name));
            Assert.AreEqual("Bar", name);
        }

        [Test]
        public static void Local()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get => this.bar;

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
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                handler.Invoke(this, args);
            }
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var argument = syntaxTree.FindInvocation("Invoke(this, args)").ArgumentList.Arguments[1];
            Assert.AreEqual(true, PropertyChangedEventArgs.TryGetPropertyNameArgument(argument.Expression, semanticModel, CancellationToken.None, out var name));
            Assert.AreEqual("propertyName", name.ToString());
        }
    }
}
