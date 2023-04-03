namespace PropertyChangedAnalyzers;

using System.Collections.Generic;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class IdentifierTypeWalker : PooledWalker<IdentifierTypeWalker>
{
    private readonly List<ParameterSyntax> parameters = new();
    private readonly List<VariableDeclaratorSyntax> locals = new();

    public override void VisitParameter(ParameterSyntax node)
    {
        this.parameters.Add(node);
        base.VisitParameter(node);
    }

    public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
    {
        this.locals.Add(node);
        base.VisitVariableDeclarator(node);
    }

    internal static bool IsLocalOrParameter(IdentifierNameSyntax candidate)
    {
        return candidate switch
        {
            { Parent: MemberAccessExpressionSyntax _ } => false,
            { Identifier: { ValueText: "value" } }
            when candidate.FirstAncestor<AccessorDeclarationSyntax>() is { Keyword: { ValueText: "set" } }
            => true,
            _ => Walk(),
        };

        bool Walk()
        {
            if (candidate.FirstAncestor<MemberDeclarationSyntax>() is { } member)
            {
                using var walker = BorrowAndVisit(member, () => new IdentifierTypeWalker());
                foreach (var parameter in walker.parameters)
                {
                    if (candidate.Identifier.ValueText == parameter.Identifier.ValueText)
                    {
                        return true;
                    }
                }

                foreach (var declarator in walker.locals)
                {
                    if (candidate.Identifier.ValueText == declarator.Identifier.ValueText)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    protected override void Clear()
    {
        this.parameters.Clear();
        this.locals.Clear();
    }
}
