namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using PropertyChangedAnalyzers.Helpers;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCorrectEqualityCodeFixProvider))]
    [Shared]
    internal class UseCorrectEqualityCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            INPC006UseReferenceEquals.DiagnosticId,
            INPC006UseObjectEqualsForReferenceTypes.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
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

                var ifStatement = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                            .FirstAncestorOrSelf<IfStatementSyntax>();
                if (!IsIfReturn(ifStatement))
                {
                    continue;
                }

                var setter = ifStatement.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
                if (setter?.IsKind(SyntaxKind.SetAccessorDeclaration) != true)
                {
                    continue;
                }

                var propertyDeclaration = setter.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
                var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);

                if (property == null)
                {
                    continue;
                }

                if (!Property.TryGetBackingField(property, semanticModel, context.CancellationToken, out var backingField))
                {
                    continue;
                }

                if (Property.TryFindValue(setter, semanticModel, context.CancellationToken, out var value) &&
                    CanFix(ifStatement, semanticModel, context.CancellationToken, value, backingField, property))
                {
                    var fieldAccess = backingField.Name.StartsWith("_")
                                          ? backingField.Name
                                          : $"this.{backingField.Name}";

                    var equalsExpression = SyntaxFactory.ParseExpression(
                                                            Snippet.EqualityCheck(
                                                                property.Type,
                                                                "value",
                                                                fieldAccess,
                                                                semanticModel))
                                                        .WithSimplifiedNames();

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            $"Use {equalsExpression}",
                            cancellationToken => Task.FromResult(context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(ifStatement.Condition, equalsExpression))),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }

        private static bool IsIfReturn(IfStatementSyntax ifStatement)
        {
            if (ifStatement == null)
            {
                return false;
            }

            if (ifStatement.Statement is ReturnStatementSyntax)
            {
                return true;
            }

            var blockSyntax = ifStatement.Statement as BlockSyntax;
            return blockSyntax?.Statements.Count == 1 && blockSyntax.Statements[0] is ReturnStatementSyntax;
        }

        private static bool CanFix(IfStatementSyntax ifStatement, SemanticModel semanticModel, CancellationToken cancellationToken, IParameterSymbol value, IFieldSymbol backingField, IPropertySymbol property)
        {
            foreach (var member in new ISymbol[] { backingField, property })
            {
                if (Equality.IsOperatorEquals(ifStatement.Condition, semanticModel, cancellationToken, value, member) ||
                    Equality.IsObjectEquals(ifStatement.Condition, semanticModel, cancellationToken, value, member) ||
                    Equality.IsEqualityComparerEquals(ifStatement.Condition, semanticModel, cancellationToken, value, member) ||
                    Equality.IsReferenceEquals(ifStatement.Condition, semanticModel, cancellationToken, value, member) ||
                    Equality.IsInstanceEquals(ifStatement.Condition, semanticModel, cancellationToken, value, member) ||
                    Equality.IsInstanceEquals(ifStatement.Condition, semanticModel, cancellationToken, member, value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}