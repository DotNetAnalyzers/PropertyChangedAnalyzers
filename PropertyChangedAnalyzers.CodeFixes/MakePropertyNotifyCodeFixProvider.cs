namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;
    using PropertyChangedAnalyzers.Helpers;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyNotifyCodeFixProvider))]
    [Shared]
    internal class MakePropertyNotifyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC002MutablePublicPropertyShouldNotify.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => DocumentOnlyFixAllProvider.Default;

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

                var propertyDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<PropertyDeclarationSyntax>();
                var typeDeclaration = propertyDeclaration?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                if (typeDeclaration == null)
                {
                    continue;
                }

                var type = semanticModel.GetDeclaredSymbolSafe(typeDeclaration, context.CancellationToken);
                if (Property.IsMutableAutoProperty(propertyDeclaration) ||
                    IsSimpleAssignmentOnly(propertyDeclaration, out _, out _))
                {
                    if (PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out var invoker) &&
                        invoker.Parameters.Length == 1)
                    {
                        if (invoker.Parameters[0].Type == KnownSymbol.String ||
                            invoker.Parameters[0].Type == KnownSymbol.PropertyChangedEventArgs)
                        {
                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    "Convert to notifying property.",
                                    cancellationToken => MakeNotifyAsync(context.Document, propertyDeclaration, invoker, semanticModel, cancellationToken),
                                    this.GetType().FullName + invoker.Name + invoker.Parameters[0].Type.Name),
                                diagnostic);
                        }
                    }
                }
            }
        }

        private static async Task<Document> MakeNotifyAsync(Document document, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol invoker, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return document;
            }

            var usesUnderscoreNames = propertyDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
            var fieldAccess = "missing";
            if (Property.IsMutableAutoProperty(propertyDeclaration))
            {
                var backingField = editor.AddBackingField(propertyDeclaration, usesUnderscoreNames, cancellationToken);
                fieldAccess = usesUnderscoreNames
                    ? backingField.Name()
                    : $"this.{backingField.Name()}";
            }
            else if (IsSimpleAssignmentOnly(propertyDeclaration, out var assignStatement, out var field))
            {
                fieldAccess = field.ToFullString();
            }

            var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
            using (var pooled = StringBuilderPool.Borrow())
            {
                var code = pooled.Item.AppendLine($"public {property.Type.ToDisplayString()} {property.Name}")
                                 .AppendLine("{")
                                 .AppendLine("    get")
                                 .AppendLine("    {")
                                 .AppendLine($"        return {fieldAccess};")
                                 .AppendLine("    }")
                                 .AppendLine()
                                 .AppendLine("    set")
                                 .AppendLine("    {")
                                 .AppendLine($"        if ({Code.EqualityCheck(property.Type, "value", fieldAccess)})")
                                 .AppendLine("        {")
                                 .AppendLine($"           return;")
                                 .AppendLine("        }")
                                 .AppendLine()
                                 .AppendLine($"        {fieldAccess} = value;")
                                 .AppendLine($"        {Code.OnPropertyChanged(invoker, property, usesUnderscoreNames)};")
                                 .AppendLine("    }")
                                 .AppendLine("}")
                                 .ToString();
                editor.ReplaceNode(propertyDeclaration, ParseProperty(code).WithLeadingTrivia(propertyDeclaration.GetLeadingTrivia()));
                return editor.GetChangedDocument();
            }
        }

        private static PropertyDeclarationSyntax ParseProperty(string code)
        {
            return (PropertyDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code)
                                                           .Members
                                                           .Single()
                                                           .WithSimplifiedNames()
                                                           .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                                                           .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                           .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static bool IsSimpleAssignmentOnly(PropertyDeclarationSyntax propertyDeclaration, out ExpressionStatementSyntax assignStatement, out ExpressionSyntax fieldAccess)
        {
            fieldAccess = null;
            assignStatement = null;
            if (!propertyDeclaration.TryGetSetAccessorDeclaration(out var setter) ||
                setter.Body == null ||
                setter.Body.Statements.Count != 1)
            {
                return false;
            }

            if (Property.AssignsValueToBackingField(setter, out var assignment))
            {
                assignStatement = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
                fieldAccess = assignment.Left;
                return assignStatement != null;
            }

            return false;
        }
    }
}
