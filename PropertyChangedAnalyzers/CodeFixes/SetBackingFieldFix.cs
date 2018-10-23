namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SetBackingFieldFix))]
    [Shared]
    internal class SetBackingFieldFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC014SetBackingField.DiagnosticId);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNode(diagnostic, out AssignmentExpressionSyntax assignment) &&
                    assignment.TryFirstAncestor(out ConstructorDeclarationSyntax ctor) &&
                    Property.TryGetAssignedProperty(assignment, out var propertyDeclaration) &&
                    Property.TryGetBackingFieldFromSetter(propertyDeclaration, semanticModel, context.CancellationToken,
                                                          out var field))
                {
                    context.RegisterCodeFix(
                        "Set backing field.",
                        (e, cancellationToken) => e.ReplaceNode(assignment.Left, x => Replacement(x)),
                        "Set backing field.",
                        diagnostic);

                    ExpressionSyntax Replacement(ExpressionSyntax x)
                    {
                        switch (x)
                        {
                            case IdentifierNameSyntax _ when IsShadowed():
                                return SyntaxFactory.ParseExpression($"this.{field.Name}")
                                                    .WithTriviaFrom(x);
                            case IdentifierNameSyntax identifierName:
                                return identifierName.WithIdentifier(SyntaxFactory.Identifier(field.Name));
                            case MemberAccessExpressionSyntax memberAccess
                                when memberAccess.Name is IdentifierNameSyntax name:
                                return memberAccess.ReplaceNode(
                                    name, name.WithIdentifier(SyntaxFactory.Identifier(field.Name)));
                            default:
                                return x;
                        }
                    }

                    bool IsShadowed()
                    {
                        if (ctor.ParameterList.Parameters.TryFirst(x => x.Identifier.ValueText == field.Name, out _))
                        {
                            return true;
                        }

                        using (var walker = VariableDeclaratorWalker.Borrow(ctor))
                        {
                            return walker.VariableDeclarators.TryFirst(x => x.Identifier.ValueText == field.Name, out _);
                        }
                    }
                }
            }
        }
    }
}
