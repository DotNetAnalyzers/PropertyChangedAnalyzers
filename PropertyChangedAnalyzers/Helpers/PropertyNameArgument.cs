namespace PropertyChangedAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct PropertyNameArgument
    {
        internal readonly ArgumentSyntax? Argument;
        internal readonly string? Name;

        internal PropertyNameArgument(ArgumentSyntax? argument, string? name)
        {
            this.Argument = argument;
            this.Name = name;
        }

        internal static PropertyNameArgument? Match(ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return argument.TryGetStringValue(semanticModel, cancellationToken, out var name)
                ? new PropertyNameArgument(argument, name)
                : (PropertyNameArgument?)null;
        }
    }
}
