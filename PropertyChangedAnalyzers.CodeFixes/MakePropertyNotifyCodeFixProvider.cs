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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyNotifyCodeFixProvider))]
    [Shared]
    internal class MakePropertyNotifyCodeFixProvider : CodeFixProvider
    {
        private const string NotifyWhenValueChanges = "Notify when value changes.";

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(INPC002MutablePublicPropertyShouldNotify.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => DocumentEditorFixAllProvider.Default;

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
                var classDeclarationSyntax = propertyDeclaration?.Parent as ClassDeclarationSyntax;
                if (classDeclarationSyntax == null)
                {
                    continue;
                }

                var type = semanticModel.GetDeclaredSymbolSafe(classDeclarationSyntax, context.CancellationToken);
                if (PropertyChanged.TryGetSetAndRaiseMethod(type, semanticModel, context.CancellationToken, out var setAndRaiseMethod))
                {
                    var key = $"{setAndRaiseMethod.ContainingType.MetadataName}.{setAndRaiseMethod.MetadataName}.";
                    if (Property.IsMutableAutoProperty(propertyDeclaration, out _, out _))
                    {
                        context.RegisterCodeFix(
                            new DocumentEditorAction(
                                key,
                                context.Document,
                                (editor, cancellationToken) => MakeAutoPropertySet(
                                    editor,
                                    propertyDeclaration,
                                    setAndRaiseMethod,
                                    semanticModel,
                                    cancellationToken),
                                key),
                            diagnostic);
                    }
                    else if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out _, out _, out _))
                    {
                        context.RegisterCodeFix(
                            new DocumentEditorAction(
                                key,
                                context.Document,
                                (editor, cancellationToken) => MakeWithBackingFieldSet(
                                    editor,
                                    propertyDeclaration,
                                    setAndRaiseMethod,
                                    semanticModel,
                                    cancellationToken),
                                key),
                            diagnostic);
                    }
                }

                if (PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out var invoker) &&
                    invoker.Parameters.Length == 1)
                {
                    if (invoker.Parameters[0].Type == KnownSymbol.String ||
                        invoker.Parameters[0].Type == KnownSymbol.PropertyChangedEventArgs)
                    {
                        if (Property.IsMutableAutoProperty(propertyDeclaration, out _, out _))
                        {
                            context.RegisterCodeFix(
                                new DocumentEditorAction(
                                    NotifyWhenValueChanges,
                                    context.Document,
                                    (editor, cancellationToken) => MakeAutoPropertyNotifyWhenValueChanges(
                                        editor,
                                        propertyDeclaration,
                                        invoker,
                                        semanticModel,
                                        cancellationToken),
                                    NotifyWhenValueChanges),
                                diagnostic);
                        }
                        else if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out _, out _, out _))
                        {
                            context.RegisterCodeFix(
                                new DocumentEditorAction(
                                    NotifyWhenValueChanges,
                                    context.Document,
                                    (editor, cancellationToken) => MakeWithBackingFieldNotifyWhenValueChanges(editor, propertyDeclaration, invoker, semanticModel, cancellationToken),
                                    NotifyWhenValueChanges),
                                diagnostic);

                            context.RegisterCodeFix(
                                new DocumentEditorAction(
                                    "Notify.",
                                    context.Document,
                                    (editor, cancellationToken) => MakeWithBackingFieldNotify(editor, propertyDeclaration, invoker, semanticModel, cancellationToken),
                                    "Notify."),
                                diagnostic);
                        }
                    }
                }
            }
        }

        private static void MakeAutoPropertyNotifyWhenValueChanges(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol invoker, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (Property.IsMutableAutoProperty(propertyDeclaration, out var getter, out var setter))
            {
                if (getter.Body != null ||
                    getter.ContainsSkippedText ||
                    setter.Body != null ||
                    setter.ContainsSkippedText)
                {
                    return;
                }

                var usesUnderscoreNames = propertyDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
                var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                var backingField = editor.AddBackingField(propertyDeclaration, usesUnderscoreNames, cancellationToken);
                var fieldAccess = usesUnderscoreNames
                    ? backingField.Name()
                    : $"this.{backingField.Name()}";
                var code = StringBuilderPool.Borrow()
                                            .AppendLine($"public Type PropertyName")
                                            .AppendLine("{")
                                            .AppendLine("    get")
                                            .AppendLine("    {")
                                            .AppendLine($"        return {fieldAccess};")
                                            .AppendLine("    }")
                                            .AppendLine()
                                            .AppendLine("    set")
                                            .AppendLine("    {")
                                            .AppendLine($"        if ({Snippet.EqualityCheck(property.Type, "value", fieldAccess, semanticModel)})")
                                            .AppendLine("        {")
                                            .AppendLine($"           return;")
                                            .AppendLine("        }")
                                            .AppendLine()
                                            .AppendLine($"        {fieldAccess} = value;")
                                            .AppendLine($"        {Snippet.OnPropertyChanged(invoker, property, usesUnderscoreNames)}")
                                            .AppendLine("    }")
                                            .AppendLine("}")
                                            .Return();
                var template = ParseProperty(code);
                editor.ReplaceNode(
                    propertyDeclaration.AccessorList,
                    propertyDeclaration.AccessorList
                                       .ReplaceNodes(
                                           new[] { getter, setter },
                                           (x, _) => x.IsKind(SyntaxKind.GetAccessorDeclaration)
                                               ? getter.WithBody(
                                                           template.Getter()
                                                                   .Body)
                                                       .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                                                       .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                                                       .WithAdditionalAnnotations(Formatter.Annotation)
                                               : setter.WithBody(
                                                           template.Setter()
                                                                   .Body)
                                                       .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                                                       .WithAdditionalAnnotations(Formatter.Annotation))
                                       .WithAdditionalAnnotations(Formatter.Annotation));
                if (propertyDeclaration.Initializer != null)
                {
                    editor.ReplaceNode(
                        propertyDeclaration,
                        x => x.WithoutInitializer());
                }
            }
        }

        private static void MakeAutoPropertySet(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol setAndRaise, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return;
            }

            if (Property.IsMutableAutoProperty(propertyDeclaration, out var getter, out var setter))
            {
                if (getter.Body != null ||
                    getter.ContainsSkippedText ||
                    setter.Body != null ||
                    setter.ContainsSkippedText)
                {
                    return;
                }

                var usesUnderscoreNames = propertyDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
                var backingField = editor.AddBackingField(propertyDeclaration, usesUnderscoreNames, cancellationToken);
                var fieldAccess = usesUnderscoreNames
                    ? backingField.Name()
                    : $"this.{backingField.Name()}";
                var code = StringBuilderPool.Borrow()
                                            .AppendLine($"public Type PropertyName")
                                            .AppendLine("{")
                                            .AppendLine($"    get {{ return {fieldAccess}; }}")
                                            .AppendLine($"    set {{ {(usesUnderscoreNames ? string.Empty : "this.")}{setAndRaise.Name}(ref {fieldAccess}, value); }}")
                                            .AppendLine("}")
                                            .Return();
                var template = ParseProperty(code);
                editor.ReplaceNode(
                    propertyDeclaration.AccessorList,
                    propertyDeclaration.AccessorList
                                       .ReplaceNodes(
                                           new[] { getter, setter },
                                           (x, _) => x.IsKind(SyntaxKind.GetAccessorDeclaration)
                                               ? getter.WithBody(
                                                           template.Getter()
                                                                   .Body)
                                                       .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                                                       .WithAdditionalAnnotations(Formatter.Annotation)
                                               : setter.WithBody(
                                                           template.Setter()
                                                                   .Body)
                                                       .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                                                       .WithAdditionalAnnotations(Formatter.Annotation))
                                       .WithAdditionalAnnotations(Formatter.Annotation));
                if (propertyDeclaration.Initializer != null)
                {
                    editor.ReplaceNode(
                        propertyDeclaration,
                        (node, g) => ((PropertyDeclarationSyntax)node).WithoutInitializer());
                }
            }
        }

        private static void MakeWithBackingFieldNotifyWhenValueChanges(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol invoker, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return;
            }

            if (propertyDeclaration.TryGetGetAccessorDeclaration(out var getter) &&
                propertyDeclaration.TryGetSetAccessorDeclaration(out var setter))
            {
                if (getter.Body?.Statements.Count != 1 ||
                    setter.Body?.Statements.Count != 1)
                {
                    return;
                }

                if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out var statement, out var assignment, out _))
                {
                    var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                    {
                        var code = StringBuilderPool.Borrow().AppendLine($"        if ({Snippet.EqualityCheck(property.Type, "value", assignment.Left.ToString(), semanticModel)})")
                                         .AppendLine("        {")
                                         .AppendLine($"           return;")
                                         .AppendLine("        }")
                                         .AppendLine()
                                         .Return();
                        var ifStatement = SyntaxFactory.ParseStatement(code)
                                                       .WithSimplifiedNames()
                                                       .WithLeadingElasticLineFeed()
                                                       .WithTrailingElasticLineFeed()
                                                       .WithAdditionalAnnotations(Formatter.Annotation);
                        editor.InsertBefore(
                            statement,
                            ifStatement);
                        var usesUnderscoreNames = propertyDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
                        var notifyStatement = SyntaxFactory.ParseStatement(Snippet.OnPropertyChanged(invoker, property, usesUnderscoreNames))
                                                                     .WithSimplifiedNames()
                                                                     .WithLeadingElasticLineFeed()
                                                                     .WithTrailingElasticLineFeed()
                                                                     .WithAdditionalAnnotations(Formatter.Annotation);
                        editor.InsertAfter(statement, notifyStatement);
                        editor.FormatNode(propertyDeclaration);
                    }
                }
            }
        }

        private static void MakeWithBackingFieldNotify(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol invoker, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return;
            }

            if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out var statement, out _, out _))
            {
                var usesUnderscoreNames = propertyDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
                var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                var notifyStatement = SyntaxFactory
                    .ParseStatement(Snippet.OnPropertyChanged(invoker, property, usesUnderscoreNames))
                    .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                    .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                    .WithSimplifiedNames()
                    .WithAdditionalAnnotations(Formatter.Annotation);
                editor.InsertAfter(statement, notifyStatement);
                editor.FormatNode(propertyDeclaration);
                return;
            }
        }

        private static void MakeWithBackingFieldSet(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol setAndRaise, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return;
            }

            if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out _, out var assignment, out var fieldAccess))
            {
                var usesUnderscoreNames = propertyDeclaration.UsesUnderscoreNames(semanticModel, cancellationToken);
                var setExpression = SyntaxFactory.ParseExpression($"{(usesUnderscoreNames ? string.Empty : "this.")}{setAndRaise.Name}(ref {fieldAccess}, value);")
                    .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                    .WithSimplifiedNames()
                    .WithAdditionalAnnotations(Formatter.Annotation);
                editor.ReplaceNode(assignment, setExpression);
            }
        }

        private static PropertyDeclarationSyntax ParseProperty(string code)
        {
            return (PropertyDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code)
                                                           .Members
                                                           .Single()
                                                           .WithSimplifiedNames()
                                                           .WithLeadingElasticLineFeed()
                                                           .WithTrailingElasticLineFeed()
                                                           .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static bool IsSimpleAssignmentOnly(PropertyDeclarationSyntax propertyDeclaration, out AccessorDeclarationSyntax setter, out ExpressionStatementSyntax statement, out AssignmentExpressionSyntax assignment, out ExpressionSyntax fieldAccess)
        {
            if (!propertyDeclaration.TryGetSetAccessorDeclaration(out setter) ||
                setter.Body == null ||
                setter.Body.Statements.Count != 1)
            {
                setter = null;
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

            setter = null;
            fieldAccess = null;
            statement = null;
            assignment = null;
            return false;
        }
    }
}
