namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading.Tasks;

    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Rename;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameFix))]
    [Shared]
    internal class RenameFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.INPC017BackingFieldNameMisMatch.Id);

        public override FixAllProvider? GetFixAllProvider() => null;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document
                                             .GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is ExpressionSyntax expression &&
                    semanticModel is { } &&
                    semanticModel.TryGetSymbol(expression, context.CancellationToken, out IFieldSymbol? field) &&
                    expression.TryFirstAncestor(out PropertyDeclarationSyntax? propertyDeclaration) &&
                    semanticModel.TryGetSymbol(propertyDeclaration, context.CancellationToken, out var property))
                {
                    var documentOptions = await context.Document.GetOptionsAsync(context.CancellationToken)
                                                       .ConfigureAwait(false);
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Rename backing field",
                            c => Renamer.RenameSymbolAsync(context.Document.Project.Solution, field, Name(property, semanticModel), documentOptions, c),
                            "Rename backing field"),
                        diagnostic);
                }
            }
        }

        private static string Name(IPropertySymbol property, SemanticModel semanticModel)
        {
            var name = semanticModel.UnderscoreFields() == CodeStyleResult.Yes
                ? $"_{property.Name.ToFirstCharLower()}"
                : property.Name.ToFirstCharLower();
            while (property.ContainingType.MemberNames.Any(x => x == name))
            {
                name += "_";
            }

            if (SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
                SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None)
            {
                name = "@" + name;
            }

            return name;
        }
    }
}
