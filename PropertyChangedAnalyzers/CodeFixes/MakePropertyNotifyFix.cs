namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyNotifyFix))]
    [Shared]
    internal class MakePropertyNotifyFix : DocumentEditorCodeFixProvider
    {
        private const string NotifyWhenValueChanges = "Notify when value changes.";

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC002MutablePublicPropertyShouldNotify.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                      .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor<PropertyDeclarationSyntax>(diagnostic, out var propertyDeclaration) &&
                    propertyDeclaration.Parent is ClassDeclarationSyntax classDeclarationSyntax &&
                    semanticModel.TryGetSymbol(classDeclarationSyntax, context.CancellationToken, out var type))
                {
                    if (TrySet.TryFind(type, semanticModel, context.CancellationToken, out var trySetMethod) &&
                        TrySet.CanCreateInvocation(trySetMethod, out _))
                    {
                        if (Property.IsMutableAutoProperty(propertyDeclaration, out var getter, out var setter))
                        {
                            context.RegisterCodeFix(
                                trySetMethod.DisplaySignature(),
                                (editor, cancellationToken) => TrySet(editor, cancellationToken),
                                trySetMethod.MetadataName,
                                diagnostic);

                            async Task TrySet(DocumentEditor editor, CancellationToken cancellationToken)
                            {
                                var fieldAccess = await editor.AddBackingFieldAsync(propertyDeclaration, cancellationToken)
                                                              .ConfigureAwait(false);
                                var trySet = await editor.TrySetInvocationAsync(trySetMethod, fieldAccess, InpcFactory.Value(), propertyDeclaration, cancellationToken)
                                                         .ConfigureAwait(false);

                                _ = editor.ReplaceNode(
                                              getter,
                                              x => x.AsExpressionBody(fieldAccess))
                                          .ReplaceNode(
                                              setter,
                                              x => x.AsExpressionBody(trySet));

                                if (propertyDeclaration.Initializer != null)
                                {
                                    editor.ReplaceNode(
                                        propertyDeclaration,
                                        (node, g) => ((PropertyDeclarationSyntax)node).WithoutInitializer());
                                }

                                _ = editor.ReplaceNode(propertyDeclaration, x => x.WithAdditionalAnnotations(Formatter.Annotation));
                            }
                        }
                        else if (IsSimpleAssignmentOnly(propertyDeclaration, out _, out _, out var assignment, out _))
                        {
                            context.RegisterCodeFix(
                                trySetMethod.DisplaySignature(),
                                async (editor, cancellationToken) =>
                                {
                                    var trySet = await editor.TrySetInvocationAsync(trySetMethod, assignment.Left, assignment.Right, propertyDeclaration, cancellationToken)
                                                             .ConfigureAwait(false);
                                    _ = editor.ReplaceNode(
                                        assignment,
                                        x => trySet);
                                },
                                trySetMethod.MetadataName,
                                diagnostic);
                        }
                    }

                    if (OnPropertyChanged.TryFind(type, semanticModel, context.CancellationToken, out var invoker) &&
                        invoker.Parameters.TrySingle(out var parameter) &&
                        parameter.Type.IsEither(KnownSymbol.String, KnownSymbol.PropertyChangedEventArgs))
                    {
                        if (Property.IsMutableAutoProperty(propertyDeclaration, out var getter, out var setter))
                        {
                            context.RegisterCodeFix(
                                MakePropertyNotifyFix.NotifyWhenValueChanges,
                                (editor, cancellationToken) =>
                                    NotifyWhenValueChanges(editor, cancellationToken),
                                nameof(NotifyWhenValueChanges),
                                diagnostic);

                            void NotifyWhenValueChanges(DocumentEditor editor, CancellationToken cancellationToken)
                            {
                                var underscoreFields = semanticModel.UnderscoreFields() == CodeStyleResult.Yes;
                                var property =
                                    semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                                var backingField = editor.AddBackingField(propertyDeclaration);
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
                                                            .AppendLine(
                                                                $"        if ({Snippet.EqualityCheck(property.Type, "value", fieldAccess, semanticModel)})")
                                                            .AppendLine("        {")
                                                            .AppendLine($"           return;")
                                                            .AppendLine("        }")
                                                            .AppendLine()
                                                            .AppendLine($"        {fieldAccess} = value;")
                                                            .AppendLine(
                                                                $"        {Snippet.OnPropertyChanged(invoker, property.Name, underscoreFields)}")
                                                            .AppendLine("    }")
                                                            .AppendLine("}")
                                                            .Return();
                                var template = ParseProperty(code);
                                _ = editor.ReplaceNode(
                                    getter,
                                    x => x.WithExpressionBody(template.Getter()
                                                                      .ExpressionBody)
                                          .WithTrailingElasticLineFeed()
                                          .WithAdditionalAnnotations(Formatter.Annotation));
                                _ = editor.ReplaceNode(
                                    setter,
                                    x => x.WithBody(template.Setter()
                                                            .Body)
                                          .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                                          .WithAdditionalAnnotations(Formatter.Annotation));
                                if (propertyDeclaration.Initializer != null)
                                {
                                    _ = editor.ReplaceNode(
                                        propertyDeclaration,
                                        x => x.WithoutInitializer());
                                }

                                _ = editor.ReplaceNode(propertyDeclaration, x => x.WithAdditionalAnnotations(Formatter.Annotation));
                            }
                        }
                        else if (IsSimpleAssignmentOnly(propertyDeclaration, out setter, out _, out var assignment, out _))
                        {
                            context.RegisterCodeFix(
                                MakePropertyNotifyFix.NotifyWhenValueChanges,
                                (editor, cancellationToken) => NotifyWhenValueChanges(editor, cancellationToken),
                                nameof(NotifyWhenValueChanges),
                                diagnostic);

                            context.RegisterCodeFix(
                                "Notify.",
                                (editor, cancellationToken) => Notify(editor, cancellationToken),
                                nameof(Notify),
                                diagnostic);

                            async Task NotifyWhenValueChanges(DocumentEditor editor, CancellationToken cancellationToken)
                            {
                                if (setter.ExpressionBody != null)
                                {
                                    var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                                    var underscoreFields = semanticModel.UnderscoreFields() == CodeStyleResult.Yes;
                                    var code = StringBuilderPool.Borrow()
                                                                .AppendLine($"public Type PropertyName")
                                                                .AppendLine("{")
                                                                .AppendLine($"    get => {assignment.Left};")
                                                                .AppendLine()
                                                                .AppendLine("    set")
                                                                .AppendLine("    {")
                                                                .AppendLine(
                                                                    $"        if ({Snippet.EqualityCheck(property.Type, "value", assignment.Left.ToString(), semanticModel)})")
                                                                .AppendLine("        {")
                                                                .AppendLine($"           return;")
                                                                .AppendLine("        }")
                                                                .AppendLine()
                                                                .AppendLine($"        {assignment};")
                                                                .AppendLine(
                                                                    $"        {Snippet.OnPropertyChanged(invoker, property.Name, underscoreFields)}")
                                                                .AppendLine("    }")
                                                                .AppendLine("}")
                                                                .Return();
                                    var template = ParseProperty(code);
                                    editor.ReplaceNode(
                                        setter,
                                        (x, _) =>
                                        {
                                            var old = (AccessorDeclarationSyntax)x;
                                            return old.WithBody(template.Setter()
                                                                        .Body)
                                                      .WithExpressionBody(null)
                                                      .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
                                        });
                                    _ = editor.FormatNode(propertyDeclaration);
                                }

                                if (setter.Body is BlockSyntax body &&
                                    body.Statements.TrySingle(out var statement))
                                {
                                    editor.InsertBefore(
                                        statement,
                                        InpcFactory.IfReturn(
                                            InpcFactory.Equals(assignment.Right, assignment.Left, editor.SemanticModel, cancellationToken)));
                                    var onPropertyChanged = await editor.OnPropertyChangedInvocationStatementAsync(invoker, propertyDeclaration, cancellationToken)
                                                         .ConfigureAwait(false);
                                    editor.InsertAfter(statement, onPropertyChanged);
                                    _ = editor.FormatNode(propertyDeclaration);
                                }
                            }

                            void Notify(DocumentEditor editor, CancellationToken cancellationToken)
                            {
                                var underscoreFields = semanticModel.UnderscoreFields() == CodeStyleResult.Yes;
                                var property =
                                    semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, cancellationToken);
                                var notifyStatement = SyntaxFactory
                                                      .ParseStatement(
                                                          Snippet.OnPropertyChanged(
                                                              invoker, property.Name, underscoreFields))
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
                                    _ = editor.FormatNode(propertyDeclaration);
                                }
                                else if (setter.Body is BlockSyntax body &&
                                         body.Statements.TrySingle(out var statement))
                                {
                                    editor.InsertAfter(statement, notifyStatement);
                                    _ = editor.FormatNode(propertyDeclaration);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static PropertyDeclarationSyntax ParseProperty(string code)
        {
            return Parse.PropertyDeclaration(code)
                        .WithSimplifiedNames()
                        .WithLeadingElasticLineFeed()
                        .WithTrailingElasticLineFeed()
                        .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static bool IsSimpleAssignmentOnly(PropertyDeclarationSyntax propertyDeclaration, out AccessorDeclarationSyntax setter, out ExpressionStatementSyntax statement, out AssignmentExpressionSyntax assignment, out ExpressionSyntax fieldAccess)
        {
            if (propertyDeclaration.TryGetSetter(out setter))
            {
                if (setter.Body?.Statements.Count == 1)
                {
                    if (Setter.AssignsValueToBackingField(setter, out assignment))
                    {
                        statement = assignment.FirstAncestorOrSelf<ExpressionStatementSyntax>();
                        fieldAccess = assignment.Left;
                        return statement != null;
                    }
                }

                if (setter.ExpressionBody != null)
                {
                    if (Setter.AssignsValueToBackingField(setter, out assignment))
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
