namespace PropertyChangedAnalyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.CodeFixExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SetBackingFieldFix))]
[Shared]
internal class SetBackingFieldFix : DocumentEditorCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.INPC014SetBackingFieldInConstructor.Id);

    protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
    {
        var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                      .ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            if (syntaxRoot?.FindNode(diagnostic.Location.SourceSpan) is AssignmentExpressionSyntax assignment &&
                diagnostic.AdditionalLocations.TrySingle(out var additionalLocation) &&
                syntaxRoot.FindNode(additionalLocation.SourceSpan) is ExpressionSyntax fieldAccess)
            {
                context.RegisterCodeFix(
                    "Set backing field.",
                    (e, cancellationToken) => e.ReplaceNode(
                        assignment.Left,
                        (x, _) => Qualify(fieldAccess).WithTriviaFrom(x)),
                    nameof(SetBackingFieldFix),
                    diagnostic);

                ExpressionSyntax Qualify(ExpressionSyntax expression)
                {
                    if (expression is IdentifierNameSyntax identifierName &&
                        (Scope.HasParameter(assignment, identifierName.Identifier.ValueText) ||
                         Scope.HasLocal(assignment, identifierName.Identifier.ValueText)))
                    {
                        return InpcFactory.SymbolAccess(identifierName.Identifier.ValueText, CodeStyleResult.Yes);
                    }

                    return expression;
                }
            }
        }
    }
}
