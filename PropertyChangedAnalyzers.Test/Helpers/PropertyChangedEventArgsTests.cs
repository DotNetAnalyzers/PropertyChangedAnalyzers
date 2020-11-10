namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static class PropertyChangedEventArgsTests
    {
        [TestCase("private static readonly PropertyChangedEventArgs Cached = new PropertyChangedEventArgs(\"P\");")]
        [TestCase("private static readonly PropertyChangedEventArgs Cached = new PropertyChangedEventArgs(nameof(P));")]
        [TestCase("public static PropertyChangedEventArgs Cached { get; } = new PropertyChangedEventArgs(nameof(P));")]
        public static void Cached(string cached)
        {
            var code = @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs Cached = new PropertyChangedEventArgs(""P"");

        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.p;
            set
            {
                if (value == this.p)
                {
                    return;
                }

                this.p = value;
                this.OnPropertyChanged(Cached);
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }
}".AssertReplace("private static readonly PropertyChangedEventArgs Cached = new PropertyChangedEventArgs(\"P\");", cached);

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var argument = syntaxTree.FindInvocation("this.OnPropertyChanged(Cached)").ArgumentList.Arguments[0];
            var findPropertyName = PropertyChangedEventArgs.Match(argument.Expression, semanticModel, CancellationToken.None)?.PropertyName(semanticModel, CancellationToken.None);
            Assert.AreEqual("P", findPropertyName?.Name);
        }

        [Test]
        public static void Local()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                @"
namespace N
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class C : INotifyPropertyChanged
    {
        private int p;

        public event PropertyChangedEventHandler PropertyChanged;

        public int P
        {
            get => this.p;
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
            Assert.AreEqual("propertyName", PropertyChangedEventArgs.Match(argument.Expression, semanticModel, CancellationToken.None)?.Argument.ToString());
        }
    }
}
