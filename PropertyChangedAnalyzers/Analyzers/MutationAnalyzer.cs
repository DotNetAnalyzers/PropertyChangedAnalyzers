﻿namespace PropertyChangedAnalyzers;

using System.Collections.Generic;
using System.Collections.Immutable;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class MutationAnalyzer : DiagnosticAnalyzer
{
    internal const string PropertyNameKey = "PropertyName";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.INPC003NotifyForDependentProperty);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(c => HandlePrefixUnaryExpression(c), SyntaxKind.PreIncrementExpression);
        context.RegisterSyntaxNodeAction(c => HandlePrefixUnaryExpression(c), SyntaxKind.PreDecrementExpression);

        context.RegisterSyntaxNodeAction(c => HandlePostfixUnaryExpression(c), SyntaxKind.PostIncrementExpression);
        context.RegisterSyntaxNodeAction(c => HandlePostfixUnaryExpression(c), SyntaxKind.PostDecrementExpression);

        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.AndAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.OrAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.ExclusiveOrAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.AddAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.DivideAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.LeftShiftAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.ModuloAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.MultiplyAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.RightShiftAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.SubtractAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => HandleAssignmentExpression(c), SyntaxKind.SimpleAssignmentExpression);

        context.RegisterSyntaxNodeAction(c => HandleArgument(c), SyntaxKind.Argument);
    }

    private static void HandlePostfixUnaryExpression(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            !IsInIgnoredScope(context) &&
            context.Node is PostfixUnaryExpressionSyntax { Operand: { } operand } postfix &&
            context is { ContainingSymbol.ContainingType: { } containingType } &&
            AssignedExpression(containingType, operand) is { } backing)
        {
            Handle(postfix, backing, context);
        }
    }

    private static void HandlePrefixUnaryExpression(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            !IsInIgnoredScope(context) &&
            context.Node is PrefixUnaryExpressionSyntax { Operand: { } operand } prefix &&
            context is { ContainingSymbol.ContainingType: { } containingType } &&
            AssignedExpression(containingType, operand) is { } backing)
        {
            Handle(prefix, backing, context);
        }
    }

    private static void HandleAssignmentExpression(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            !IsInIgnoredScope(context) &&
            context.Node is AssignmentExpressionSyntax { Left: { } left } assignment &&
            context is { ContainingSymbol.ContainingType: { } containingType } &&
            AssignedExpression(containingType, left) is { } backing)
        {
            Handle(assignment, backing, context);
        }
    }

    private static void HandleArgument(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            !IsInIgnoredScope(context) &&
            context.Node is ArgumentSyntax { Expression: { } expression } argument &&
            argument.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword) &&
            context is { ContainingSymbol.ContainingType: { } containingType } &&
            AssignedExpression(containingType, expression) is { } backing)
        {
            Handle(argument.Expression, backing, context);
        }
    }

    private static void Handle(ExpressionSyntax mutation, ExpressionSyntax backing, SyntaxNodeAnalysisContext context)
    {
        if (context is { ContainingSymbol.ContainingType: { } containingType } &&
            containingType.IsAssignableTo(KnownSymbol.INotifyPropertyChanged, context.Compilation) &&
            mutation.TryFirstAncestorOrSelf<TypeDeclarationSyntax>(out var typeDeclaration))
        {
            using var pathWalker = MemberPath.PathWalker.Borrow(backing);
            if (!pathWalker.TryFirst(out var firstTokenInPath) ||
                firstTokenInPath.Parent is null)
            {
                return;
            }

            var firstIdentifierNameSymbol = context.SemanticModel.GetSymbolInfo(firstTokenInPath.Parent, context.CancellationToken).Symbol;
            if (!TypeSymbolComparer.Equal(firstIdentifierNameSymbol?.ContainingType, containingType))
            {
                return;
            }

            foreach (var member in typeDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax propertyDeclaration &&
                    propertyDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword) &&
                    ExpressionBodyOrGetter(propertyDeclaration) is { } getter &&
                    !getter.Contains(backing) &&
                    PropertyPath.Uses(getter, pathWalker, context) &&
                    !Property.IsLazy(propertyDeclaration, context.SemanticModel, context.CancellationToken) &&
                    context.SemanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken) is { } property &&
                    PropertyChanged.InvokesPropertyChangedFor(mutation, property, context.SemanticModel, context.CancellationToken) == AnalysisResult.No)
                {
                    if (context.Node.TryFirstAncestor(out PropertyDeclarationSyntax? inProperty) &&
                        ReferenceEquals(inProperty, propertyDeclaration) &&
                        Property.FindSingleReturned(inProperty) is { } returned &&
                        PropertyPath.Uses(backing, returned, context))
                    {
                        // We let INPC002 handle this
                        continue;
                    }

                    var properties = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string?>(PropertyNameKey, property.Name), });
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.INPC003NotifyForDependentProperty, context.Node.GetLocation(), properties, property.Name));
                }
            }
        }
    }

    private static bool IsInIgnoredScope(SyntaxNodeAnalysisContext context)
    {
        return context switch
        {
            { ContainingSymbol: IMethodSymbol { Name: "Dispose" } } => true,
            { Node.Parent: ObjectCreationExpressionSyntax _ } => true,
            { ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.Constructor } } => !context.Node.TryFirstAncestor<AnonymousFunctionExpressionSyntax>(out _),
            _ => false,
        };
    }

    private static ExpressionSyntax? AssignedExpression(ITypeSymbol containingType, SyntaxNode node)
    {
        return node switch
        {
            IdentifierNameSyntax identifierName
                when !IdentifierTypeWalker.IsLocalOrParameter(identifierName) &&
                     !containingType.TryFindProperty(identifierName.Identifier.ValueText, out _)
                => identifierName,
            MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax _, Name: { } name } memberAccess
                when !containingType.TryFindProperty(name.Identifier.ValueText, out _)
                => memberAccess,
            MemberAccessExpressionSyntax memberAccess => memberAccess,
            _ => null,
        };
    }

    private static SyntaxNode? ExpressionBodyOrGetter(PropertyDeclarationSyntax property)
    {
        return property switch
        {
            { ExpressionBody.Expression: { } expression } => expression,
            { AccessorList: { } } when property.TryGetGetter(out var getter) => (SyntaxNode?)getter.Body ??
                getter.ExpressionBody,
            _ => null,
        };
    }
}
