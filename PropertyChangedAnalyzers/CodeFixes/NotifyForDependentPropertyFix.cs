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
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

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
            var underscoreFields = semanticModel.UnderscoreFields() == CodeStyleResult.Yes;
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.TryGetValue(MutationAnalyzer.PropertyNameKey, out var propertyName))
                {
                    if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ExpressionSyntax expression) &&
                        expression.TryFirstAncestor(out ClassDeclarationSyntax classDeclaration) &&
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
                                            if (expressionStatement.TryFirstAncestor(out AccessorDeclarationSyntax setter))
                                            {
                                                _ = editor.FormatNode(setter);
                                            }
                                        },
                                        nameof(NotifyForDependentPropertyFix),
                                        diagnostic);
                                    continue;
                            }

                            if (trySet.Parent is ArrowExpressionClauseSyntax)
                            {
                                context.RegisterCodeFix(
                                    $"Notify that property {propertyName} changes.",
                                    (editor, cancellationToken) => MakeNotifyCreateIf(
                                        editor,
                                        trySet,
                                        propertyName,
                                        onPropertyChangedMethod,
                                        underscoreFields),
                                    nameof(NotifyForDependentPropertyFix),
                                    diagnostic);
                                continue;
                            }

                            if (trySet.Parent is IfStatementSyntax ifStatement)
                            {
                                context.RegisterCodeFix(
                                    $"Notify that property {propertyName} changes.",
                                    (editor, _) => MakeNotifyInIf(
                                        editor,
                                        ifStatement,
                                        propertyName,
                                        onPropertyChangedMethod,
                                        underscoreFields),
                                    nameof(NotifyForDependentPropertyFix),
                                    diagnostic);
                                continue;
                            }

                            if (trySet.Parent is PrefixUnaryExpressionSyntax unary &&
                                unary.IsKind(SyntaxKind.LogicalNotExpression) &&
                                unary.Parent is IfStatementSyntax ifStatement2 &&
                                ifStatement2.IsReturnOnly())
                            {
                                context.RegisterCodeFix(
                                    $"Notify that property {propertyName} changes.",
                                    (editor, _) => MakeNotify(
                                        editor,
                                        expression,
                                        propertyName,
                                        onPropertyChangedMethod,
                                        underscoreFields),
                                    nameof(NotifyForDependentPropertyFix),
                                    diagnostic);
                                continue;
                            }
                        }

                        context.RegisterCodeFix(
                            $"Notify that property {propertyName} changes.",
                            (editor, _) => MakeNotify(
                                editor,
                                expression,
                                propertyName,
                                onPropertyChangedMethod,
                                underscoreFields),
                            nameof(NotifyForDependentPropertyFix),
                            diagnostic);
                    }
                }
            }
        }

        private static void MakeNotify(DocumentEditor editor, ExpressionSyntax assignment, string propertyName, IMethodSymbol invoker, bool usesUnderscoreNames)
        {
            var snippet = assignment.FirstAncestor<PropertyDeclarationSyntax>() is PropertyDeclarationSyntax propertyDeclaration &&
                propertyDeclaration.Identifier.ValueText == propertyName
                    ? Snippet.OnPropertyChanged(invoker, propertyName, usesUnderscoreNames)
                    : Snippet.OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames);
            var onPropertyChanged = SyntaxFactory.ParseStatement(snippet)
                                                 .WithSimplifiedNames()
                                                 .WithLeadingElasticLineFeed().WithTrailingElasticLineFeed()
                                                 .WithAdditionalAnnotations(Formatter.Annotation);
            if (assignment.Parent is AnonymousFunctionExpressionSyntax anonymousFunction)
            {
                if (anonymousFunction.Body is BlockSyntax block)
                {
                    if (block.Statements.Count > 1)
                    {
                        var previousStatement = InsertAfter(block, block.Statements.Last(), invoker);
                        editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
                    }

                    return;
                }

                var expressionStatement = (ExpressionStatementSyntax)editor.Generator.ExpressionStatement(anonymousFunction.Body);
                var withStatements = editor.Generator.WithStatements(anonymousFunction, new[] { expressionStatement, onPropertyChanged });
                editor.ReplaceNode(anonymousFunction, withStatements);
            }
            else if (assignment.Parent is ExpressionStatementSyntax assignStatement &&
                     assignStatement.Parent is BlockSyntax assignBlock)
            {
                var previousStatement = InsertAfter(assignBlock, assignStatement, invoker);
                editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
            }
            else if (assignment.Parent is PrefixUnaryExpressionSyntax unary &&
                     unary.IsKind(SyntaxKind.LogicalNotExpression) &&
                     unary.Parent is IfStatementSyntax ifStatement &&
                     ifStatement.Parent is BlockSyntax ifBlock)
            {
                var previousStatement = InsertAfter(ifBlock, ifStatement, invoker);
                editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
            }
        }

        private static void MakeNotifyCreateIf(DocumentEditor editor, InvocationExpressionSyntax invocation, string propertyName, IMethodSymbol invoker, bool usesUnderscoreNames)
        {
            if (invocation.Parent is ExpressionStatementSyntax assignStatement &&
                assignStatement.Parent is BlockSyntax)
            {
                editor.ReplaceNode(
                    assignStatement,
                    (node, _) =>
                    {
                        var code = StringBuilderPool.Borrow()
                                                    .AppendLine($"if ({invocation.ToFullString().TrimEnd('\r', '\n')})")
                                                    .AppendLine("{")
                                                    .AppendLine($"    {Snippet.OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames)}")
                                                    .AppendLine("}")
                                                    .Return();

                        return SyntaxFactory.ParseStatement(code)
                                            .WithSimplifiedNames()
                                            .WithLeadingElasticLineFeed().WithTrailingElasticLineFeed()
                                            .WithAdditionalAnnotations(Formatter.Annotation);
                    });
                _ = editor.FormatNode(invocation.FirstAncestorOrSelf<PropertyDeclarationSyntax>());
                return;
            }

            if (invocation.Parent is ArrowExpressionClauseSyntax arrow &&
                arrow.Parent is AccessorDeclarationSyntax accessor)
            {
                editor.RemoveNode(accessor.ExpressionBody);
                _ = editor.ReplaceNode(
                    accessor,
                    x =>
                    {
                        var code = StringBuilderPool.Borrow()
                                                    .AppendLine($"if ({invocation.ToFullString().TrimEnd('\r', '\n')})")
                                                    .AppendLine("{")
                                                    .AppendLine($"    {Snippet.OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames)}")
                                                    .AppendLine("}")
                                                    .Return();

                        var body = SyntaxFactory.ParseStatement(code)
                                                .WithSimplifiedNames()
                                                .WithLeadingElasticLineFeed().WithTrailingElasticLineFeed()
                                                .WithAdditionalAnnotations(Formatter.Annotation);
                        return x.WithBody(SyntaxFactory.Block(body))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
                    });
                _ = editor.FormatNode(invocation.FirstAncestorOrSelf<PropertyDeclarationSyntax>());
            }
        }

        private static void MakeNotifyInIf(DocumentEditor editor, IfStatementSyntax ifStatement, string propertyName, IMethodSymbol invoker, bool usesUnderscoreNames)
        {
            var onPropertyChanged = SyntaxFactory.ParseStatement(Snippet.OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames))
                                                 .WithSimplifiedNames()
                                                 .WithLeadingElasticLineFeed().WithTrailingElasticLineFeed()
                                                 .WithAdditionalAnnotations(Formatter.Annotation);

            if (ifStatement.Statement is BlockSyntax block)
            {
                if (block.Statements.Count == 0)
                {
                    editor.ReplaceNode(
                        block,
                        block.AddStatements(onPropertyChanged));
                    return;
                }

                var previousStatement = InsertAfter(block, ifStatement, invoker);
                editor.InsertAfter(previousStatement, new[] { onPropertyChanged });
                return;
            }

            if (ifStatement.Statement != null)
            {
                editor.ReplaceNode(
                    ifStatement.Statement,
                    (node, _) =>
                    {
                        var code = StringBuilderPool.Borrow()
                                                    .AppendLine("{")
                                                    .AppendLine($"{ifStatement.Statement.ToFullString().TrimEnd('\r', '\n')}")
                                                    .AppendLine($"    {Snippet.OnOtherPropertyChanged(invoker, propertyName, usesUnderscoreNames)}")
                                                    .AppendLine("}")
                                                    .Return();

                        return SyntaxFactory.ParseStatement(code)
                                            .WithSimplifiedNames().WithTrailingElasticLineFeed()
                                            .WithAdditionalAnnotations(Formatter.Annotation);
                    });
            }
        }

        private static StatementSyntax InsertAfter(BlockSyntax block, StatementSyntax assignStatement, IMethodSymbol invoker)
        {
            var index = block.Statements.IndexOf(assignStatement);
            var previousStatement = assignStatement;
            for (var i = index + 1; i < block.Statements.Count; i++)
            {
                if (block.Statements[i] is ExpressionStatementSyntax expressionStatement &&
                    expressionStatement.Expression is InvocationExpressionSyntax invocation)
                {
                    var identifierName = invocation.Expression as IdentifierNameSyntax;
                    if (identifierName == null)
                    {
                        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                        if (!(memberAccess?.Expression is ThisExpressionSyntax))
                        {
                            break;
                        }

                        identifierName = memberAccess.Name as IdentifierNameSyntax;
                    }

                    if (identifierName == null)
                    {
                        break;
                    }

                    if (identifierName.Identifier.ValueText == invoker.Name)
                    {
                        previousStatement = expressionStatement;
                    }
                }
            }

            return previousStatement;
        }
    }
}
