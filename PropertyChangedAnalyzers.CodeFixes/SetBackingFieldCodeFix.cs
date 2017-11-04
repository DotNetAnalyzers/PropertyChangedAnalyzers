namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SetBackingFieldCodeFix))]
    [Shared]
    internal class SetBackingFieldCodeFix : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC014PreferSettingBackingFieldInCtor.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var assignment = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                                      .FirstAncestorOrSelf<AssignmentExpressionSyntax>();
                if (assignment != null)
                {
                    context.RegisterDocumentEditorFix(
                        "Set backing field.",
                        (e, _) => SetBackingField(e, assignment),
                        diagnostic);
                }
            }
        }

        private static void SetBackingField(DocumentEditor editor, AssignmentExpressionSyntax assignment)
        {
            editor.ReplaceNode(
                assignment.Left,
                (x, _) =>
                {
                    if (Property.TryGetAssignedProperty((AssignmentExpressionSyntax)x.Parent, out var propertyDeclaration) &&
                        propertyDeclaration.TryGetSetAccessorDeclaration(out var setter) &&
                        Property.TryGetSingleAssignmentInSetter(setter, out var fieldAssignment))
                    {
                        return fieldAssignment.Left.WithLeadingTriviaFrom(x);
                    }

                    return x;
                });
        }
    }
}