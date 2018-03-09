namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
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
                        context.RegisterDocumentEditorFix(
                            key,
                            (editor, cancellationToken) => MakeAutoPropertySet(
                                editor,
                                propertyDeclaration,
                                setAndRaiseMethod,
                                semanticModel,
                                cancellationToken),
                            key,
                            diagnostic);
                    }
                    else if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out _, out _, out _))
                    {
                        context.RegisterDocumentEditorFix(
                            key,
                            (editor, cancellationToken) => MakeWithBackingFieldSet(
                                editor,
                                propertyDeclaration,
                                setAndRaiseMethod,
                                semanticModel),
                            key,
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
                            context.RegisterDocumentEditorFix(
                                NotifyWhenValueChanges,
                                (editor, cancellationToken) => MakeAutoPropertyNotifyWhenValueChanges(editor, propertyDeclaration, invoker, semanticModel, cancellationToken),
                                NotifyWhenValueChanges,
                                diagnostic);
                        }
                        else if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out _, out _, out _))
                        {
                            context.RegisterDocumentEditorFix(
                                NotifyWhenValueChanges,
                                (editor, cancellationToken) => MakeWithBackingFieldNotifyWhenValueChanges(editor, propertyDeclaration, invoker, semanticModel, cancellationToken),
                                NotifyWhenValueChanges,
                                diagnostic);

                            context.RegisterDocumentEditorFix(
                                "Notify.",
                                (editor, cancellationToken) => MakeWithBackingFieldNotify(editor, propertyDeclaration, invoker, semanticModel, cancellationToken),
                                "Notify.",
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

                var underscoreFields = CodeStyle.UnderscoreFields(semanticModel);
                var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                var backingField = editor.AddBackingField(propertyDeclaration, underscoreFields, cancellationToken);
                var fieldAccess = underscoreFields
                    ? backingField.Name()
                    : $"this.{backingField.Name()}";
                var code = StringBuilderPool.Borrow()
                                            .AppendLine($"public Type PropertyName")
                                            .AppendLine("{")
                                            .AppendLine($"    get => {fieldAccess};")
                                            .AppendLine()
                                            .AppendLine("    set")
                                            .AppendLine("    {")
                                            .AppendLine($"        if ({Snippet.EqualityCheck(property.Type, "value", fieldAccess, semanticModel)})")
                                            .AppendLine("        {")
                                            .AppendLine($"           return;")
                                            .AppendLine("        }")
                                            .AppendLine()
                                            .AppendLine($"        {fieldAccess} = value;")
                                            .AppendLine($"        {Snippet.OnPropertyChanged(invoker, property.Name, underscoreFields)}")
                                            .AppendLine("    }")
                                            .AppendLine("}")
                                            .Return();
                var template = ParseProperty(code);
                editor.ReplaceNode(
                    getter,
                    x => x.WithExpressionBody(template.Getter().ExpressionBody)
                          .WithTrailingElasticLineFeed()
                          .WithAdditionalAnnotations(Formatter.Annotation));
                editor.ReplaceNode(
                    setter,
                    x => x.WithBody(template.Setter().Body)
                          .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                          .WithAdditionalAnnotations(Formatter.Annotation));
                if (propertyDeclaration.Initializer != null)
                {
                    editor.ReplaceNode(
                        propertyDeclaration,
                        x => x.WithoutInitializer());
                }

                editor.ReplaceNode(propertyDeclaration, x => x.WithAdditionalAnnotations(Formatter.Annotation));
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

                var underscoreFields = CodeStyle.UnderscoreFields(semanticModel);
                var backingField = editor.AddBackingField(propertyDeclaration, underscoreFields, cancellationToken);
                var fieldAccess = underscoreFields
                    ? backingField.Name()
                    : $"this.{backingField.Name()}";
                var code = StringBuilderPool.Borrow()
                                            .AppendLine($"public Type PropertyName")
                                            .AppendLine("{")
                                            .AppendLine($"    get => {fieldAccess};")
                                            .AppendLine($"    set => {(underscoreFields ? string.Empty : "this.")}{setAndRaise.Name}(ref {fieldAccess}, value);")
                                            .AppendLine("}")
                                            .Return();
                var template = ParseProperty(code);
                editor.ReplaceNode(
                    getter,
                    x => x.WithExpressionBody(template.Getter().ExpressionBody)
                          .WithLeadingLineFeed());

                editor.ReplaceNode(
                    setter,
                    x => x.WithExpressionBody(template.Setter().ExpressionBody)
                          .WithLeadingLineFeed()
                          .WithTrailingLineFeed());

                if (propertyDeclaration.Initializer != null)
                {
                    editor.ReplaceNode(
                        propertyDeclaration,
                        (node, g) => ((PropertyDeclarationSyntax)node).WithoutInitializer());
                }

                editor.ReplaceNode(propertyDeclaration, x => x.WithAdditionalAnnotations(Formatter.Annotation));
            }
        }

        private static void MakeWithBackingFieldNotifyWhenValueChanges(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol invoker, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return;
            }

            if (propertyDeclaration.TryGetSetAccessorDeclaration(out var setter))
            {
                if (setter.ExpressionBody != null &&
                    IsSimpleAssignmentOnly(propertyDeclaration, out _, out _, out var assignment, out _))
                {
                    var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                    var underscoreFields = CodeStyle.UnderscoreFields(semanticModel);
                    var code = StringBuilderPool.Borrow()
                                                .AppendLine($"public Type PropertyName")
                                                .AppendLine("{")
                                                .AppendLine($"    get => {assignment.Left};")
                                                .AppendLine()
                                                .AppendLine("    set")
                                                .AppendLine("    {")
                                                .AppendLine($"        if ({Snippet.EqualityCheck(property.Type, "value", assignment.Left.ToString(), semanticModel)})")
                                                .AppendLine("        {")
                                                .AppendLine($"           return;")
                                                .AppendLine("        }")
                                                .AppendLine()
                                                .AppendLine($"        {assignment};")
                                                .AppendLine($"        {Snippet.OnPropertyChanged(invoker, property.Name, underscoreFields)}")
                                                .AppendLine("    }")
                                                .AppendLine("}")
                                                .Return();
                    var template = ParseProperty(code);
                    editor.ReplaceNode(
                        setter,
                        (x, _) =>
                        {
                            var old = (AccessorDeclarationSyntax)x;
                            return old.WithBody(template.Setter().Body)
                                      .WithExpressionBody(null)
                                      .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
                        });
                    editor.FormatNode(propertyDeclaration);
                }

                if (setter.Body?.Statements.Count == 1 &&
                    IsSimpleAssignmentOnly(propertyDeclaration, out _, out var statement, out assignment, out _))
                {
                    var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                    var code = StringBuilderPool.Borrow()
                                                .AppendLine($"        if ({Snippet.EqualityCheck(property.Type, "value", assignment.Left.ToString(), semanticModel)})")
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
                    var underscoreFields = CodeStyle.UnderscoreFields(semanticModel);
                    var notifyStatement = SyntaxFactory
                                          .ParseStatement(
                                              Snippet.OnPropertyChanged(invoker, property.Name, underscoreFields))
                                          .WithSimplifiedNames()
                                          .WithLeadingElasticLineFeed()
                                          .WithTrailingElasticLineFeed()
                                          .WithAdditionalAnnotations(Formatter.Annotation);
                    editor.InsertAfter(statement, notifyStatement);
                    editor.FormatNode(propertyDeclaration);
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

            if (IsSimpleAssignmentOnly(propertyDeclaration, out var setter, out var statement, out var assignment, out _))
            {
                var underscoreFields = CodeStyle.UnderscoreFields(semanticModel);
                var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                var notifyStatement = SyntaxFactory
                    .ParseStatement(Snippet.OnPropertyChanged(invoker, property.Name, underscoreFields))
                    .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
                    .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                    .WithSimplifiedNames()
                    .WithAdditionalAnnotations(Formatter.Annotation);
                if (setter.ExpressionBody != null)
                {
                    editor.ReplaceNode(
                        setter,
                        (x, _) =>
                        {
                            var old = (AccessorDeclarationSyntax)x;
                            return old.WithBody(
                                          SyntaxFactory.Block(
                                              SyntaxFactory.ExpressionStatement(assignment),
                                              notifyStatement))
                                      .WithExpressionBody(null)
                                      .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
                        });
                    editor.FormatNode(propertyDeclaration);
                }
                else if (setter.Body != null)
                {
                    editor.InsertAfter(statement, notifyStatement);
                    editor.FormatNode(propertyDeclaration);
                }
            }
        }

        private static void MakeWithBackingFieldSet(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclaration, IMethodSymbol setAndRaise, SemanticModel semanticModel)
        {
            var classDeclaration = propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return;
            }

            if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out _, out var assignment, out var fieldAccess))
            {
                var underscoreFields = CodeStyle.UnderscoreFields(semanticModel);
                var setExpression = SyntaxFactory.ParseExpression($"{(underscoreFields ? string.Empty : "this.")}{setAndRaise.Name}(ref {fieldAccess}, value);")
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
            if (propertyDeclaration.TryGetSetAccessorDeclaration(out setter))
            {
                if (setter.Body?.Statements.Count == 1)
                {
                    if (Property.AssignsValueToBackingField(setter, out assignment))
                    {
                        statement = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
                        fieldAccess = assignment.Left;
                        return statement != null;
                    }
                }

                if (setter.ExpressionBody != null)
                {
                    if (Property.AssignsValueToBackingField(setter, out assignment))
                    {
                        statement = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
                        fieldAccess = assignment.Left;
                        return true;
                    }
                }
            }

            setter = null;
            fieldAccess = null;
            statement = null;
            assignment = null;
            return false;
        }
    }
}
