namespace PropertyChangedAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct GetAndSetResult
    {
        internal readonly bool Same;

        internal readonly ExpressionSyntax Get;

        internal readonly ExpressionSyntax Set;

        public GetAndSetResult(bool same, ExpressionSyntax get, ExpressionSyntax set)
        {
            this.Same = same;
            this.Get = get;
            this.Set = set;
        }
    }
}
