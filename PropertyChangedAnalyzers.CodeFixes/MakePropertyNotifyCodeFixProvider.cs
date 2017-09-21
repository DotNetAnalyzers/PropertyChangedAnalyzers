﻿namespace PropertyChangedAnalyzers
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
    using Microsoft.CodeAnalysis.Simplification;
    using PropertyChangedAnalyzers.Helpers;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyNotifyCodeFixProvider))]
    [Shared]
    internal class MakePropertyNotifyCodeFixProvider : CodeFixProvider
    {
        private const string NotifyWhenValueChanges = "Notify when value changes.";

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
                if (PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out var invoker) &&
                    invoker.Parameters.Length == 1)
                {
                    if (invoker.Parameters[0].Type == KnownSymbol.String ||
                        invoker.Parameters[0].Type == KnownSymbol.PropertyChangedEventArgs)
                    {
                        if (Property.IsMutableAutoProperty(propertyDeclaration, out _, out _))
                        {
                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    NotifyWhenValueChanges,
                                    cancellationToken => MakeAutoPropertyNotifyAsync(context.Document, propertyDeclaration, invoker, semanticModel, cancellationToken),
                                    NotifyWhenValueChanges),
                                diagnostic);
                        }
                        else if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out _, out _))
                        {
                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    NotifyWhenValueChanges,
                                    cancellationToken => MakeWithBackingFieldNotifyWhenValueChangesAsync(context.Document, propertyDeclaration, invoker, semanticModel, cancellationToken),
                                    NotifyWhenValueChanges),
                                diagnostic);

                            context.RegisterCodeFix(
                                CodeAction.Create(
                                    "Notify.",
                                    cancellationToken => MakeWithBackingFieldNotifyAsync(context.Document, propertyDeclaration, invoker, semanticModel, cancellationToken),
                                    "Notify."),
                                diagnostic);
                        }
                    }
                }
            }
        }

        private static async Task<Document> MakeAutoPropertyNotifyAsync(Document document, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol invoker, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return document;
            }

            if (Property.IsMutableAutoProperty(propertyDeclaration, out var getter, out var setter))
            {
                if (getter.Body != null ||
                    getter.ContainsSkippedText ||
                    setter.Body != null ||
                    setter.ContainsSkippedText)
                {
                    return document;
                }

                var usesUnderscoreNames = propertyDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
                var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                var backingField = editor.AddBackingField(propertyDeclaration, usesUnderscoreNames, cancellationToken);
                var fieldAccess = usesUnderscoreNames
                    ? backingField.Name()
                    : $"this.{backingField.Name()}";
                using (var pooled = StringBuilderPool.Borrow())
                {
                    var code = pooled.Item.AppendLine($"public Type PropertyName")
                                     .AppendLine("{")
                                     .AppendLine("    get")
                                     .AppendLine("    {")
                                     .AppendLine($"        return {fieldAccess};")
                                     .AppendLine("    }")
                                     .AppendLine()
                                     .AppendLine("    set")
                                     .AppendLine("    {")
                                     .AppendLine($"        if ({Snippet.EqualityCheck(property.Type, "value", fieldAccess)})")
                                     .AppendLine("        {")
                                     .AppendLine($"           return;")
                                     .AppendLine("        }")
                                     .AppendLine()
                                     .AppendLine($"        {fieldAccess} = value;")
                                     .AppendLine($"        {Snippet.OnPropertyChanged(invoker, property, usesUnderscoreNames)};")
                                     .AppendLine("    }")
                                     .AppendLine("}")
                                     .ToString();
                    var template = ParseProperty(code);
                    editor.ReplaceNode(
                        propertyDeclaration.AccessorList,
                        propertyDeclaration.AccessorList
                                           .ReplaceNodes(
                                               new[] { getter, setter },
                                               (x, _) => x.IsKind(SyntaxKind.GetAccessorDeclaration)
                                                   ? getter.WithBody(template.Getter().Body)
                                                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                                                           .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                           .WithAdditionalAnnotations(Formatter.Annotation)
                                                   : setter.WithBody(template.Setter().Body)
                                                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                                                           .WithAdditionalAnnotations(Formatter.Annotation))
                                           .WithAdditionalAnnotations(Formatter.Annotation));
                    if (propertyDeclaration.Initializer != null)
                    {
                        editor.ReplaceNode(
                            propertyDeclaration,
                            (node, g) => ((PropertyDeclarationSyntax)node).WithoutInitializer());
                    }

                    return editor.GetChangedDocument();
                }
            }

            return document;
        }

        private static async Task<Document> MakeWithBackingFieldNotifyWhenValueChangesAsync(Document document, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol invoker, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return document;
            }

            if (propertyDeclaration.TryGetGetAccessorDeclaration(out var getter) &&
                propertyDeclaration.TryGetSetAccessorDeclaration(out var setter))
            {
                if (getter.Body?.Statements.Count != 1 ||
                    setter.Body?.Statements.Count != 1)
                {
                    return document;
                }

                if (IsSimpleAssignmentOnly(propertyDeclaration, out var statement, out var assignment, out _))
                {
                    var usesUnderscoreNames = propertyDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
                    var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                    using (var pooled = StringBuilderPool.Borrow())
                    {
                        var code = pooled.Item.AppendLine($"public {property.Type.ToDisplayString()} {property.Name}")
                                         .AppendLine("{")
                                         .AppendLine("    get")
                                         .AppendLine("    {")
                                         .AppendLine($"        return {assignment.Left};")
                                         .AppendLine("    }")
                                         .AppendLine()
                                         .AppendLine("    set")
                                         .AppendLine("    {")
                                         .AppendLine($"        if ({Snippet.EqualityCheck(property.Type, "value", assignment.Left.ToString())})")
                                         .AppendLine("        {")
                                         .AppendLine($"           return;")
                                         .AppendLine("        }")
                                         .AppendLine()
                                         .AppendLine(statement.ToFullString())
                                         .AppendLine($"        {Snippet.OnPropertyChanged(invoker, property, usesUnderscoreNames)};")
                                         .AppendLine("    }")
                                         .AppendLine("}")
                                         .ToString();
                        var template = ParseProperty(code);
                        editor.ReplaceNode(
                            propertyDeclaration.AccessorList,
                            propertyDeclaration.AccessorList
                                               .ReplaceNodes(
                                                   new[] { getter, setter },
                                                   (x, _) => x.IsKind(SyntaxKind.GetAccessorDeclaration)
                                                       ? getter.WithBody(template.Getter().Body)
                                                               .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                                                               .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                               .WithAdditionalAnnotations(Formatter.Annotation)
                                                       : setter.WithBody(template.Setter().Body)
                                                               .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                                                               .WithAdditionalAnnotations(Formatter.Annotation))
                                               .WithAdditionalAnnotations(Formatter.Annotation));
                        return editor.GetChangedDocument();
                    }
                }
            }

            return document;
        }

        private static async Task<Document> MakeWithBackingFieldNotifyAsync(Document document, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol invoker, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken)
                                             .ConfigureAwait(false);
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return document;
            }

            if (IsSimpleAssignmentOnly(propertyDeclaration, out var statement, out _, out _))
            {
                var usesUnderscoreNames = propertyDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
                var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                var notifyStatement = SyntaxFactory
                    .ParseStatement($"                {Snippet.OnPropertyChanged(invoker, property, usesUnderscoreNames)};")
                    .WithSimplifiedNames()
                    .WithAdditionalAnnotations(Formatter.Annotation);
                editor.InsertAfter(statement, notifyStatement);
                return editor.GetChangedDocument();
            }

            return document;
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

        private static bool IsSimpleAssignmentOnly(PropertyDeclarationSyntax propertyDeclaration, out ExpressionStatementSyntax statement, out AssignmentExpressionSyntax assignment, out ExpressionSyntax fieldAccess)
        {
            if (!propertyDeclaration.TryGetSetAccessorDeclaration(out var setter) ||
                setter.Body == null ||
                setter.Body.Statements.Count != 1)
            {
                fieldAccess = null;
                statement = null;
                assignment = null;
                return false;
            }

            if (Property.AssignsValueToBackingField(setter, out assignment))
            {
                statement = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
                fieldAccess = assignment.Left;
                return statement != null;
            }

            fieldAccess = null;
            statement = null;
            assignment = null;
            return false;
        }
    }
}
