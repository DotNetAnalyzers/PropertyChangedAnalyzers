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
                                var trySet = await editor.TrySetInvocationAsync(trySetMethod, fieldAccess, InpcFactory.Value, propertyDeclaration, cancellationToken)
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
                        if (Property.IsMutableAutoProperty(propertyDeclaration, out var getter, out var setter) &&
                            semanticModel.TryGetSymbol(propertyDeclaration, context.CancellationToken, out var property))
                        {
                            context.RegisterCodeFix(
                                MakePropertyNotifyFix.NotifyWhenValueChanges,
                                (editor, cancellationToken) =>
                                    NotifyWhenValueChanges(editor, cancellationToken),
                                nameof(NotifyWhenValueChanges),
                                diagnostic);

                            async Task NotifyWhenValueChanges(DocumentEditor editor, CancellationToken cancellationToken)
                            {
                                var backingField = await editor.AddBackingFieldAsync(propertyDeclaration, cancellationToken)
                                                               .ConfigureAwait(false);
                                var onPropertyChanged = await editor.OnPropertyChangedInvocationStatementAsync(invoker, propertyDeclaration, cancellationToken)
                                                                    .ConfigureAwait(false);
                                _ = editor.ReplaceNode(
                                    getter,
                                    x => x.AsExpressionBody(backingField)
                                          .WithTrailingLineFeed());
                                _ = editor.ReplaceNode(
                                    setter,
                                    x => x.AsBlockBody(
                                        InpcFactory.IfReturn(
                                            InpcFactory.Equals(property.Type, InpcFactory.Value, backingField, editor.SemanticModel)),
                                        SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, backingField, InpcFactory.Value)),
                                        onPropertyChanged));
                                if (propertyDeclaration.Initializer != null)
                                {
                                    _ = editor.ReplaceNode(
                                        propertyDeclaration,
                                        x => x.WithoutInitializer());
                                }

                                _ = editor.FormatNode(propertyDeclaration);
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
                                    var onPropertyChanged = await editor.OnPropertyChangedInvocationStatementAsync(invoker, propertyDeclaration, cancellationToken)
                                                                        .ConfigureAwait(false);
                                    _ = editor.ReplaceNode(
                                        setter,
                                        x => x.AsBlockBody(
                                            InpcFactory.IfReturn(
                                                InpcFactory.Equals(assignment.Right, assignment.Left, editor.SemanticModel, cancellationToken)),
                                            SyntaxFactory.ExpressionStatement(assignment),
                                            onPropertyChanged));
                                    _ = editor.FormatNode(propertyDeclaration);
                                }
                                else if (setter.Body is BlockSyntax body &&
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

                            async Task Notify(DocumentEditor editor, CancellationToken cancellationToken)
                            {
                                if (setter.ExpressionBody != null)
                                {
                                    var onPropertyChanged = await editor.OnPropertyChangedInvocationStatementAsync(invoker, propertyDeclaration, cancellationToken)
                                                                        .ConfigureAwait(false);
                                    _ = editor.ReplaceNode(
                                        setter,
                                        x => x.AsBlockBody(
                                            SyntaxFactory.ExpressionStatement(assignment),
                                            onPropertyChanged));
                                    _ = editor.FormatNode(propertyDeclaration);
                                }
                                else if (setter.Body is BlockSyntax body &&
                                         body.Statements.TrySingle(out var statement))
                                {
                                    var onPropertyChanged = await editor.OnPropertyChangedInvocationStatementAsync(invoker, propertyDeclaration, cancellationToken)
                                                                        .ConfigureAwait(false);
                                    editor.InsertAfter(statement, onPropertyChanged);
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
