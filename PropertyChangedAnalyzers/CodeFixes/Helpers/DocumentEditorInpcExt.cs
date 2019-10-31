namespace PropertyChangedAnalyzers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;

    internal static class DocumentEditorInpcExt
    {
        internal static async Task<ExpressionSyntax> AddBackingFieldAsync(this DocumentEditor editor, PropertyDeclarationSyntax property, CancellationToken cancellationToken)
        {
            var backingField = editor.AddBackingField(property);
            var qualifyFieldAccessAsync = property.Modifiers.Any(SyntaxKind.StaticKeyword)
                ? CodeStyleResult.No
                : await editor.QualifyFieldAccessAsync(cancellationToken)
                              .ConfigureAwait(false);

            return InpcFactory.SymbolAccess(backingField.Name(), qualifyFieldAccessAsync);
        }

        internal static async Task<ExpressionStatementSyntax> OnPropertyChangedInvocationStatementAsync(this DocumentEditor editor, IMethodSymbol invoker, string propertyName, CancellationToken cancellationToken)
        {
            var qualifyMethodAccess = await editor.QualifyMethodAccessAsync(cancellationToken)
                                                  .ConfigureAwait(false);
            var qualifyPropertyAccess = await editor.QualifyPropertyAccessAsync(cancellationToken)
                                                    .ConfigureAwait(false);
            return InpcFactory.OnPropertyChangedInvocationStatement(
                  InpcFactory.SymbolAccess(invoker.Name, qualifyMethodAccess),
                  InpcFactory.Nameof(InpcFactory.SymbolAccess(propertyName, qualifyPropertyAccess)));
        }

        internal static async Task<ExpressionStatementSyntax> OnPropertyChangedInvocationStatementAsync(this DocumentEditor editor, IMethodSymbol invoker, PropertyDeclarationSyntax containingProperty, CancellationToken cancellationToken)
        {
            if (invoker.Parameters.TrySingle(out var parameter))
            {
                var qualifyMethodAccess = await editor.QualifyMethodAccessAsync(cancellationToken)
                                                      .ConfigureAwait(false);
                var nameExpression = await editor.NameOfContainingAsync(containingProperty, parameter, cancellationToken)
                                                 .ConfigureAwait(false);
                return InpcFactory.OnPropertyChangedInvocationStatement(
                    InpcFactory.SymbolAccess(invoker.Name, qualifyMethodAccess),
                    nameExpression);
            }

            throw new InvalidOperationException("Could not find name parameter.");
        }

        internal static async Task<InvocationExpressionSyntax> TrySetInvocationAsync(this DocumentEditor editor, IMethodSymbol trySet, ExpressionSyntax field, ExpressionSyntax value, PropertyDeclarationSyntax containingProperty, CancellationToken cancellationToken)
        {
            if (trySet.TryFindParameter(KnownSymbol.String, out var nameParameter))
            {
                var qualifyMethodAccess = await editor.QualifyMethodAccessAsync(cancellationToken)
                                                      .ConfigureAwait(false);
                var nameExpression = await editor.NameOfContainingAsync(containingProperty, nameParameter, cancellationToken)
                                                 .ConfigureAwait(false);

                return InpcFactory.TrySetInvocation(
                    qualifyMethodAccess,
                    trySet,
                    field,
                    value,
                    nameExpression);
            }

            throw new InvalidOperationException("Could not find name parameter.");
        }

        internal static void MoveOnPropertyChangedInside(this DocumentEditor editor, IfStatementSyntax ifTrySet, ExpressionStatementSyntax onPropertyChanged, CancellationToken cancellationToken)
        {
            editor.RemoveNode(onPropertyChanged);
            editor.AddOnPropertyChanged(ifTrySet, OnPropertyChanged(), cancellationToken);

            ExpressionStatementSyntax OnPropertyChanged()
            {
                if (onPropertyChanged.HasLeadingTrivia &&
                    onPropertyChanged.GetLeadingTrivia() is SyntaxTriviaList leadingTrivia &&
                    leadingTrivia.TryFirst(out var first) &&
                    first.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    onPropertyChanged = onPropertyChanged.WithLeadingTrivia(leadingTrivia.Remove(first));
                }

                return onPropertyChanged.WithAdditionalAnnotations(Formatter.Annotation);
            }
        }

        internal static async Task AddOnPropertyChangedMethodAsync(this DocumentEditor editor, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            var qualifyAccess = classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
                ? CodeStyleResult.No
                : await editor.QualifyEventAccessAsync(cancellationToken)
                              .ConfigureAwait(false);

            _ = editor.AddMethod(
                classDeclaration,
                InpcFactory.OnPropertyChangedDeclaration(
                    qualifyAccess,
                    classDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword),
                    classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword),
                    CallerMemberNameAttribute.IsAvailable(editor.SemanticModel)));
        }

        internal static void AddOnPropertyChanged(this DocumentEditor editor, IfStatementSyntax ifTrySet, ExpressionStatementSyntax onPropertyChanged, CancellationToken cancellationToken)
        {
            switch (ifTrySet.Statement)
            {
                case BlockSyntax block:
                    editor.AddOnPropertyChanged(block, onPropertyChanged, 0, cancellationToken);
                    break;
                case ExpressionStatementSyntax expressionStatement:
                    _ = editor.ReplaceNode(
                        ifTrySet,
                        x => x.WithStatement(SyntaxFactory.Block(expressionStatement, onPropertyChanged)));
                    break;
                case EmptyStatementSyntax _:
                case null:
                    _ = editor.ReplaceNode(
                        ifTrySet,
                        x => x.WithStatement(SyntaxFactory.Block(onPropertyChanged)));
                    break;
            }
        }

        internal static void AddOnPropertyChanged(this DocumentEditor editor, ExpressionSyntax mutation, ExpressionStatementSyntax onPropertyChanged, CancellationToken cancellationToken)
        {
            switch (mutation.Parent)
            {
                case SimpleLambdaExpressionSyntax lambda
                    when lambda.Body is ExpressionSyntax:
                    editor.ReplaceNode(
                        lambda,
                        x => x.AsBlockBody(SyntaxFactory.ExpressionStatement((ExpressionSyntax)x.Body), onPropertyChanged));
                    break;
                case ParenthesizedLambdaExpressionSyntax lambda
                    when lambda.Body is ExpressionSyntax:
                    editor.ReplaceNode(
                        lambda,
                        x => x.AsBlockBody(SyntaxFactory.ExpressionStatement((ExpressionSyntax)x.Body), onPropertyChanged));
                    break;
                case ExpressionStatementSyntax expressionStatement:
                    editor.AddOnPropertyChangedAfter(expressionStatement, onPropertyChanged, cancellationToken);
                    break;
                case PrefixUnaryExpressionSyntax unary
                    when unary.IsKind(SyntaxKind.LogicalNotExpression) &&
                         unary.Parent is IfStatementSyntax ifNot:
                    editor.AddOnPropertyChangedAfter(ifNot, onPropertyChanged, cancellationToken);
                    break;
            }
        }

        internal static void AddOnPropertyChanged(this DocumentEditor editor, BlockSyntax block, ExpressionStatementSyntax onPropertyChangedStatement, int after, CancellationToken cancellationToken)
        {
            for (var i = after; i < block.Statements.Count; i++)
            {
                var statement = block.Statements[i];
                if (ShouldAddBefore(statement))
                {
                    editor.InsertBefore(
                        statement,
                        onPropertyChangedStatement);
                    return;
                }
            }

            editor.ReplaceNode(
                block,
                block.AddStatements(onPropertyChangedStatement));

            // We can't use the semantic model here as it may not track nodes added before in fixall
            bool ShouldAddBefore(StatementSyntax statement)
            {
                switch (statement)
                {
                    case ExpressionStatementSyntax expressionStatement
                        when expressionStatement.Expression is AssignmentExpressionSyntax:
                        return false;
                    case ExpressionStatementSyntax expressionStatement
                        when expressionStatement.Expression is InvocationExpressionSyntax other &&
                             onPropertyChangedStatement.Expression is InvocationExpressionSyntax onPropertyChangedInvocation:
                        return !NamesEqual(other, onPropertyChangedInvocation) ||
                                PropertyIndex(onPropertyChangedInvocation) < PropertyIndex(other);

                        bool NamesEqual(InvocationExpressionSyntax x, InvocationExpressionSyntax y)
                        {
                            return x.TryGetMethodName(out var xn) &&
                                   y.TryGetMethodName(out var yn) &&
                                   xn == yn;
                        }

                        int PropertyIndex(InvocationExpressionSyntax x)
                        {
                            if (x.ArgumentList is ArgumentListSyntax argumentList)
                            {
                                if (argumentList.Arguments.TrySingle(out var argument))
                                {
                                    switch (argument.Expression)
                                    {
                                        case LiteralExpressionSyntax literal
                                            when literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                                                 TryFindIndex(literal.Token.ValueText, out var index):
                                            return index;
                                        case InvocationExpressionSyntax nameof
                                            when nameof.IsNameOf() &&
                                                 nameof.ArgumentList.Arguments.TrySingle(out var nameofArg) &&
                                                 MemberPath.TryFindLast(nameofArg.Expression, out var last) &&
                                                 TryFindIndex(last.ValueText, out var index):
                                            return index;
                                        default:
                                            return int.MaxValue;
                                    }

                                    bool TryFindIndex(string name, out int result)
                                    {
                                        if (statement.TryFirstAncestor(out ClassDeclarationSyntax? classDeclaration) &&
                                            classDeclaration.TryFindProperty(name, out var property))
                                        {
                                            if (property.Contains(statement))
                                            {
                                                result = -1;
                                                return true;
                                            }

                                            result = classDeclaration.Members.IndexOf(property);
                                            return true;
                                        }

                                        result = default;
                                        return false;
                                    }
                                }

                                return -1;
                            }

                            return int.MaxValue;
                        }

                    default:
                        return true;
                }
            }
        }

        internal static void AddOnPropertyChangedAfter(this DocumentEditor editor, StatementSyntax statement, ExpressionStatementSyntax onPropertyChanged, CancellationToken cancellationToken)
        {
            if (statement.Parent is BlockSyntax block)
            {
                editor.AddOnPropertyChanged(block, onPropertyChanged, block.Statements.IndexOf(statement) + 1, cancellationToken);
                return;
            }

            throw new InvalidOperationException("Statement not in block. Failed adding OnPropertyChanged()");
        }

        internal static async Task<ExpressionSyntax> NameOfContainingAsync(this DocumentEditor editor, PropertyDeclarationSyntax property, IParameterSymbol parameter, CancellationToken cancellationToken)
        {
            if (parameter.IsCallerMemberName())
            {
                return null;
            }

            if (parameter.Type == KnownSymbol.String)
            {
                return await NameExpression();
            }

            if (parameter.Type == KnownSymbol.PropertyChangedEventArgs)
            {
                var expression = await NameExpression();
                return (ExpressionSyntax)editor.Generator.ObjectCreationExpression(
                    editor.Generator.TypeExpression(
                        editor.SemanticModel.Compilation.GetTypeByMetadataName(KnownSymbol.PropertyChangedEventArgs.FullName)),
                    editor.Generator.Argument(RefKind.None, expression));
            }

            throw new InvalidOperationException("Could not create name for parameter type.");

            async Task<ExpressionSyntax> NameExpression()
            {
                if (property.Modifiers.Any(SyntaxKind.StaticKeyword))
                {
                    return InpcFactory.Nameof(SyntaxFactory.IdentifierName(property.Identifier));
                }

                return InpcFactory.Nameof(
                    InpcFactory.SymbolAccess(
                        property.Identifier.ValueText,
                        await editor.QualifyEventAccessAsync(cancellationToken).ConfigureAwait(false)));
            }
        }

        internal static Task<ExpressionSyntax> SymbolAccessAsync(this DocumentEditor editor, ISymbol symbol, SyntaxNode context, CancellationToken cancellationToken)
        {
            switch (symbol)
            {
                case ILocalSymbol _:
                case IParameterSymbol _:
                    return Task.FromResult(InpcFactory.SymbolAccess(symbol.Name, CodeStyleResult.No));
                case IFieldSymbol member:
                    return FieldAccessAsync(editor, member, context, cancellationToken);
                case IEventSymbol member:
                    return EventAccessAsync(editor, member, context, cancellationToken);
                case IPropertySymbol member:
                    return PropertyAccessAsync(editor, member, context, cancellationToken);
                case IMethodSymbol member:
                    return MethodAccessAsync(editor, member, context, cancellationToken);
                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol));
            }
        }

        internal static async Task<ExpressionSyntax> FieldAccessAsync(this DocumentEditor editor, IFieldSymbol symbol, SyntaxNode context, CancellationToken cancellationToken)
        {
            if (symbol.IsConst ||
                symbol.IsStatic ||
                context.IsInStaticContext() ||
                await editor.QualifyFieldAccessAsync(cancellationToken).ConfigureAwait(false) == CodeStyleResult.No)
            {
                return InpcFactory.SymbolAccess(symbol.Name, CodeStyleResult.No);
            }

            return InpcFactory.SymbolAccess(symbol.Name, CodeStyleResult.Yes);
        }

        internal static async Task<ExpressionSyntax> EventAccessAsync(this DocumentEditor editor, IEventSymbol symbol, SyntaxNode context, CancellationToken cancellationToken)
        {
            if (symbol.IsStatic ||
                context.IsInStaticContext() ||
                await editor.QualifyEventAccessAsync(cancellationToken).ConfigureAwait(false) == CodeStyleResult.No)
            {
                return InpcFactory.SymbolAccess(symbol.Name, CodeStyleResult.No);
            }

            return InpcFactory.SymbolAccess(symbol.Name, CodeStyleResult.Yes);
        }

        internal static async Task<ExpressionSyntax> PropertyAccessAsync(this DocumentEditor editor, IPropertySymbol symbol, SyntaxNode context, CancellationToken cancellationToken)
        {
            if (symbol.IsStatic ||
                context.IsInStaticContext() ||
                await editor.QualifyPropertyAccessAsync(cancellationToken).ConfigureAwait(false) == CodeStyleResult.No)
            {
                return InpcFactory.SymbolAccess(symbol.Name, CodeStyleResult.No);
            }

            return InpcFactory.SymbolAccess(symbol.Name, CodeStyleResult.Yes);
        }

        internal static async Task<ExpressionSyntax> MethodAccessAsync(this DocumentEditor editor, IMethodSymbol symbol, SyntaxNode context, CancellationToken cancellationToken)
        {
            if (symbol.IsStatic ||
                context.IsInStaticContext() ||
                await editor.QualifyMethodAccessAsync(cancellationToken).ConfigureAwait(false) == CodeStyleResult.No)
            {
                return InpcFactory.SymbolAccess(symbol.Name, CodeStyleResult.No);
            }

            return InpcFactory.SymbolAccess(symbol.Name, CodeStyleResult.Yes);
        }
    }
}
