namespace PropertyChangedAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Simplification;

    internal static class MakePropertyNotifyHelper
    {
        internal static PropertyDeclarationSyntax WithoutInitializer(this PropertyDeclarationSyntax property)
        {
            if (property.Initializer == null)
            {
                return property;
            }

            return property.WithInitializer(null)
                           .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                           .WithAccessorList(
                               property.AccessorList.WithCloseBraceToken(
                                   property
                                       .AccessorList
                                       .CloseBraceToken
                                       .WithTrailingTrivia(
                                           property
                                               .SemicolonToken
                                               .TrailingTrivia)));
        }

        internal static StatementSyntax OnPropertyChanged(this SyntaxGenerator syntaxGenerator, string propertyName, bool useCallerMemberName, bool usedUnderscoreNames, IMethodSymbol invoker)
        {
            var prefix = usedUnderscoreNames
                             ? string.Empty
                             : "this.";
            if (invoker == null)
            {
                var eventAccess = SyntaxFactory.ParseExpression($"{prefix}PropertyChanged?.Invoke(new PropertyChangedEventArgs(nameof({propertyName}))");
                return (StatementSyntax)syntaxGenerator.ExpressionStatement(eventAccess.WithAdditionalAnnotations(Formatter.Annotation))
                                                       .WithAdditionalAnnotations(Formatter.Annotation);
            }

            var memberAccess = SyntaxFactory.ParseExpression($"{prefix}{invoker.Name}");
            if (useCallerMemberName && invoker.Parameters[0].IsCallerMemberName())
            {
                return (StatementSyntax)syntaxGenerator.ExpressionStatement(syntaxGenerator.InvocationExpression(memberAccess));
            }

            var arg = SyntaxFactory.ParseExpression($"nameof({prefix}{propertyName})").WithAdditionalAnnotations(Formatter.Annotation);
            return (StatementSyntax)syntaxGenerator.ExpressionStatement(syntaxGenerator.InvocationExpression(memberAccess, arg))
                                                   .WithAdditionalAnnotations(Formatter.Annotation);
        }

        internal static IfStatementSyntax IfValueEqualsBackingFieldReturn(this SyntaxGenerator syntaxGenerator, string fieldName, IPropertySymbol property, ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            var fieldAccess = fieldName.StartsWith("_")
                ? fieldName
                : $"this.{fieldName}";
            return IfValueEqualsBackingFieldReturn(
                syntaxGenerator,
                SyntaxFactory.ParseExpression(fieldAccess),
                property,
                diagnosticOptions);
        }

        internal static IfStatementSyntax IfValueEqualsBackingFieldReturn(this SyntaxGenerator syntaxGenerator, ExpressionSyntax fieldAccess, IPropertySymbol property, ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            if (!property.Type.IsReferenceType ||
                property.Type == KnownSymbol.String)
            {
                if (Equality.HasEqualityOperator(property.Type))
                {
                    var valueEqualsExpression = syntaxGenerator.ValueEqualsExpression(
                        SyntaxFactory.ParseName("value"),
                        fieldAccess);
                    return (IfStatementSyntax)syntaxGenerator.IfStatement(
                        valueEqualsExpression,
                        new[] { SyntaxFactory.ReturnStatement() });
                }

                foreach (var equals in property.Type.GetMembers("Equals"))
                {
                    var method = equals as IMethodSymbol;
                    if (method?.Parameters.Length == 1 &&
                        ReferenceEquals(
                            method.Parameters[0]
                                  .Type,
                            property.Type))
                    {
                        var equalsExpression = syntaxGenerator.InvocationExpression(
                            SyntaxFactory.ParseExpression("value.Equals"),
                            fieldAccess);
                        return (IfStatementSyntax)syntaxGenerator.IfStatement(
                            equalsExpression,
                            new[] { SyntaxFactory.ReturnStatement() });
                    }
                }

                if (property.Type.Name == "Nullable")
                {
                    if (Equality.HasEqualityOperator(((INamedTypeSymbol)property.Type).TypeArguments[0]))
                    {
                        var valueEqualsExpression = syntaxGenerator.ValueEqualsExpression(
                            SyntaxFactory.ParseName("value"),
                            fieldAccess);
                        return (IfStatementSyntax)syntaxGenerator.IfStatement(
                            valueEqualsExpression,
                            new[] { SyntaxFactory.ReturnStatement() });
                    }

                    var nullableEquals = syntaxGenerator.InvocationExpression(
                        SyntaxFactory.ParseExpression("System.Nullable.Equals")
                                     .WithAdditionalAnnotations(Simplifier.Annotation),
                        SyntaxFactory.ParseName("value"),
                        fieldAccess);
                    return (IfStatementSyntax)syntaxGenerator.IfStatement(
                        nullableEquals,
                        new[] { SyntaxFactory.ReturnStatement() });
                }

                var comparerEquals = syntaxGenerator.InvocationExpression(
                    SyntaxFactory.ParseExpression(
                                     $"System.Collections.Generic.EqualityComparer<{property.Type.ToDisplayString()}>.Default.Equals")
                                 .WithAdditionalAnnotations(Simplifier.Annotation),
                    SyntaxFactory.ParseName("value"),
                    fieldAccess);
                return (IfStatementSyntax)syntaxGenerator.IfStatement(
                    comparerEquals,
                    new[] { SyntaxFactory.ReturnStatement() });
            }

            var referenceEqualsExpression = syntaxGenerator.InvocationExpression(
                ReferenceTypeEquality(diagnosticOptions),
                SyntaxFactory.ParseName("value"),
                fieldAccess);
            return (IfStatementSyntax)syntaxGenerator.IfStatement(
                referenceEqualsExpression,
                new[] { SyntaxFactory.ReturnStatement() });
        }

        [Obsolete("Use snippet")]
        internal static ExpressionSyntax ReferenceTypeEquality(ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions)
        {
            if (!diagnosticOptions.TryGetValue(INPC006UseReferenceEquals.DiagnosticId, out var setting))
            {
                return SyntaxFactory.ParseExpression(nameof(ReferenceEquals));
            }

            return setting == ReportDiagnostic.Suppress
                ? SyntaxFactory.ParseExpression(nameof(Equals))
                : SyntaxFactory.ParseExpression(nameof(ReferenceEquals));
        }
    }
}