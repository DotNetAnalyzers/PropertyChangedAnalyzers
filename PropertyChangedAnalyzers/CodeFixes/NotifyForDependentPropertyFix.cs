namespace PropertyChangedAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotifyForDependentPropertyFix))]
    [Shared]
    internal class NotifyForDependentPropertyFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.INPC003NotifyForDependentProperty.Id);

        /// <inheritdoc/>
        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(MutationAnalyzer.PropertyNameKey, out var propertyName))
                {
                    if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ExpressionSyntax? expression) &&
                        expression.TryFirstAncestor(out ClassDeclarationSyntax? classDeclaration) &&
                        semanticModel.TryGetNamedType(classDeclaration, context.CancellationToken, out var type) &&
                        OnPropertyChanged.TryFind(type, semanticModel, context.CancellationToken, out var onPropertyChangedMethod) &&
                        onPropertyChangedMethod.Parameters.TrySingle(out var parameter) &&
                        parameter.Type.IsEither(KnownSymbol.String, KnownSymbol.PropertyChangedEventArgs))
                    {
                        if (expression is InvocationExpressionSyntax trySet &&
                            semanticModel.TryGetSymbol(trySet, context.CancellationToken, out var trySetMethod) &&
                            TrySet.IsMatch(trySetMethod, semanticModel, context.CancellationToken) != AnalysisResult.No)
                        {
                            switch (trySet.Parent)
                            {
                                case ExpressionStatementSyntax expressionStatement:
                                    context.RegisterCodeFix(
                                        $"Notify that property {propertyName} changes.",
                                        async (editor, cancellationToken) =>
                                        {
                                            var onPropertyChangedStatement = await editor.OnPropertyChangedInvocationStatementAsync(onPropertyChangedMethod, propertyName, cancellationToken)
                                                                                         .ConfigureAwait(false);
                                            editor.ReplaceNode(
                                                expressionStatement,
                                                InpcFactory.IfStatement(
                                                    trySet,
                                                    onPropertyChangedStatement));
                                            if (expressionStatement.TryFirstAncestor(out AccessorDeclarationSyntax? setter))
                                            {
                                                _ = editor.FormatNode(setter);
                                            }
                                        },
                                        nameof(NotifyForDependentPropertyFix),
                                        diagnostic);
                                    break;
                                case ArrowExpressionClauseSyntax arrow
                                    when arrow.Parent is AccessorDeclarationSyntax setter:
                                    context.RegisterCodeFix(
                                        $"Notify that property {propertyName} changes.",
                                        async (editor, cancellationToken) =>
                                        {
                                            var onPropertyChangedStatement = await editor.OnPropertyChangedInvocationStatementAsync(onPropertyChangedMethod, propertyName, cancellationToken)
                                                                                         .ConfigureAwait(false);
                                            _ = editor.ReplaceNode(
                                                setter,
                                                x => x.AsBlockBody(
                                                    InpcFactory.IfStatement(
                                                        trySet,
                                                        onPropertyChangedStatement)));
                                        },
                                        nameof(NotifyForDependentPropertyFix),
                                        diagnostic);
                                    break;
                                case IfStatementSyntax ifTrySet:
                                    context.RegisterCodeFix(
                                        $"Notify that property {propertyName} changes.",
                                        async (editor, cancellationToken) =>
                                        {
                                            var onPropertyChangedStatement = await editor.OnPropertyChangedInvocationStatementAsync(onPropertyChangedMethod, propertyName, cancellationToken)
                                                                                         .ConfigureAwait(false);
                                            editor.AddOnPropertyChanged(ifTrySet, onPropertyChangedStatement);
                                        },
                                        nameof(NotifyForDependentPropertyFix),
                                        diagnostic);
                                    break;
                                case PrefixUnaryExpressionSyntax unary
                                    when Gu.Roslyn.AnalyzerExtensions.Equality.IsNegated(trySet) &&
                                         unary.Parent is IfStatementSyntax ifNotTrySetReturn &&
                                         ifNotTrySetReturn.IsReturnOnly():
                                    context.RegisterCodeFix(
                                        $"Notify that property {propertyName} changes.",
                                        async (editor, cancellationToken) =>
                                        {
                                            var onPropertyChangedStatement = await editor.OnPropertyChangedInvocationStatementAsync(onPropertyChangedMethod, propertyName, cancellationToken)
                                                                                         .ConfigureAwait(false);
                                            editor.AddOnPropertyChangedAfter(ifNotTrySetReturn, onPropertyChangedStatement);
                                        },
                                        nameof(NotifyForDependentPropertyFix),
                                        diagnostic);
                                    break;
                            }
                        }
                        else
                        {
                            context.RegisterCodeFix(
                                $"Notify that property {propertyName} changes.",
                                async (editor, cancellationToken) =>
                                {
                                    var onPropertyChangedStatement = await editor.OnPropertyChangedInvocationStatementAsync(onPropertyChangedMethod, propertyName, cancellationToken)
                                                                                 .ConfigureAwait(false);
                                    editor.AddOnPropertyChanged(expression, onPropertyChangedStatement);
                                },
                                nameof(NotifyForDependentPropertyFix),
                                diagnostic);
                        }
                    }
                }
            }
        }
    }
}
