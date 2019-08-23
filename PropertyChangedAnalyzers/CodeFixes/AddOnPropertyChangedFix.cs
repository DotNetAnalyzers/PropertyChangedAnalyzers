namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddOnPropertyChangedFix))]
    [Shared]
    internal class AddOnPropertyChangedFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.INPC007MissingInvoker.Id);

        protected override DocumentEditorFixAllProvider FixAllProvider() => DocumentEditorFixAllProvider.Project;

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ClassDeclarationSyntax classDeclaration))
                {
                    context.RegisterCodeFix(
                        "Add OnPropertyChanged()",
                        (editor, cancellationToken) => editor.AddOnPropertyChangedMethodAsync(classDeclaration, cancellationToken),
                        "Add OnPropertyChanged()",
                        diagnostic);

                    if (!classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword) &&
                        !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                    {
                        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
                        if (NoPropertyShouldNotify(classDeclaration, semanticModel, context.CancellationToken))
                        {
                            context.RegisterCodeFix(
                                "Seal class.",
                                (editor, _) => editor.Seal(classDeclaration),
                                "Seal class.",
                                diagnostic);
                        }
                    }
                }
            }
        }

        private static bool NoPropertyShouldNotify(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property &&
                    Property.ShouldNotify(property, semanticModel, cancellationToken))
                {
                    return false;
                }
            }

            return true;
        }
    }
}