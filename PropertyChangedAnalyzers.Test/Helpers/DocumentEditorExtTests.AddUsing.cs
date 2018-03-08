namespace PropertyChangedAnalyzers.Test.Helpers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Editing;
    using NUnit.Framework;

    public partial class DocumentEditorExtTests
    {
        public class AddUsing
        {
            [Test]
            public async Task SystemWhenEmpty()
            {
                var testCode = @"
namespace RoslynSandbox
{
}";
                var sln = CodeFactory.CreateSolution(testCode);
                var editor = await DocumentEditor.CreateAsync(sln.Projects.First().Documents.First()).ConfigureAwait(false);

                var expected = @"
namespace RoslynSandbox
{
    using System;
}";
                var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));
                editor.AddUsing(usingDirective);
                CodeAssert.AreEqual(expected, editor.GetChangedDocument());
            }

            [Test]
            public async Task SystemWhenEmptyOutside()
            {
                var testCode = @"
namespace RoslynSandbox
{
}";

                var otherCode = @"
using System;

namespace RoslynSandbox
{
}";
                var sln = CodeFactory.CreateSolution(new[] { testCode, otherCode });
                var editor = await DocumentEditor.CreateAsync(sln.Projects.First().Documents.First()).ConfigureAwait(false);

                var expected = @"using System;

namespace RoslynSandbox
{
}";
                var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));
                editor.AddUsing(usingDirective);
                CodeAssert.AreEqual(expected, editor.GetChangedDocument());
            }

            [Test]
            public async Task SystemWhenSystemCollectionsExists()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.Collections;
}";
                var sln = CodeFactory.CreateSolution(testCode);
                var editor = await DocumentEditor.CreateAsync(sln.Projects.First().Documents.First()).ConfigureAwait(false);

                var expected = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
}";
                var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"));
                editor.AddUsing(usingDirective);
                CodeAssert.AreEqual(expected, editor.GetChangedDocument());
            }
        }
    }
}
