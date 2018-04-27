namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SetBackingFieldCodeFix))]
    [Shared]
    internal class SetBackingFieldCodeFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC014PreferSettingBackingFieldInCtor.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
                if (node is AssignmentExpressionSyntax assignment &&
                    Property.TryGetAssignedProperty(assignment, out var propertyDeclaration) &&
                    Property.TryGetBackingFieldFromSetter(propertyDeclaration, semanticModel, context.CancellationToken, out var field))
                {
                    context.RegisterCodeFix(
                        "Set backing field.",
                        (e, cancellationToken) => SetBackingField(e, assignment, field),
                        "Set backing field.",
                        diagnostic);
                }
            }
        }

        private static void SetBackingField(DocumentEditor editor, AssignmentExpressionSyntax oldAssignment, IFieldSymbol field)
        {
            editor.ReplaceNode(
                oldAssignment.Left,
                (x, _) =>
                {
                    if (x is IdentifierNameSyntax identifierName)
                    {
                        return identifierName.WithIdentifier(SyntaxFactory.Identifier(field.Name));
                    }

                    if (x is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name is IdentifierNameSyntax name)
                    {
                        return memberAccess.ReplaceNode(
                            name,
                            name.WithIdentifier(SyntaxFactory.Identifier(field.Name)));
                    }

                    return x;
                });
        }
    }
}
