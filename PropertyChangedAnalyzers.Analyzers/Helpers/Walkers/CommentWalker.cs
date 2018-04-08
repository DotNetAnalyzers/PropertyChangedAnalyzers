namespace PropertyChangedAnalyzers.Helpers.Walkers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    internal class CommentWalker : PooledWalker<CommentWalker>
    {
        private readonly List<SyntaxTrivia> singleLineComments = new List<SyntaxTrivia>();
        private readonly List<SyntaxTrivia> multiLineComments = new List<SyntaxTrivia>();

        public CommentWalker(SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node)
        : base(depth)
        {
        }

        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                this.singleLineComments.Add(trivia);
            }

            if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                this.multiLineComments.Add(trivia);
            }

            base.VisitTrivia(trivia);
        }

        internal static IEnumerable<string> GetComments(SyntaxNode node)
        {
            if (node == null)
            {
                yield break;
            }

            using (var walker = BorrowAndVisit(node, () => new CommentWalker(SyntaxWalkerDepth.Trivia)))
            {
                foreach (var singleComment in walker.singleLineComments)
                {
                    if (singleComment.SyntaxTree.TryGetText(out var text))
                    {
                        yield return StripCommentSymbols(text.GetSubText(singleComment.Span).ToString());
                    }
                }

                foreach (var multiComment in walker.multiLineComments)
                {
                    yield return null;
                }
            }
        }

        protected override void Clear()
        {
            this.singleLineComments.Clear();
            this.multiLineComments.Clear();
        }

        private static string StripCommentSymbols(string comment)
        {
            return comment.TrimStart('/', '*').Trim();
        }
    }
}
