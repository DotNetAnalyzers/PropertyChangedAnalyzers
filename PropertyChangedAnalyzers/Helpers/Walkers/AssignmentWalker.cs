﻿namespace PropertyChangedAnalyzers;

using System.Collections.Generic;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class AssignmentWalker : PooledWalker<AssignmentWalker>
{
    private readonly List<AssignmentExpressionSyntax> assignments = new();

    private AssignmentWalker()
    {
    }

    /// <summary>
    /// Gets a list with all <see cref="AssignmentExpressionSyntax"/> in the scope.
    /// </summary>
    internal IReadOnlyList<AssignmentExpressionSyntax> Assignments => this.assignments;

    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        this.assignments.Add(node);
        base.VisitAssignmentExpression(node);
    }

    internal static AssignmentWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new AssignmentWalker());

    protected override void Clear()
    {
        this.assignments.Clear();
    }
}
