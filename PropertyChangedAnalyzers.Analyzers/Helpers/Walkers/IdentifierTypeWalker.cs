namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class IdentifierTypeWalker : PooledWalker<IdentifierTypeWalker>
    {
        private readonly List<ParameterSyntax> parameters = new List<ParameterSyntax>();
        private readonly List<VariableDeclaratorSyntax> variableDeclarators = new List<VariableDeclaratorSyntax>();

        public override void VisitParameter(ParameterSyntax node)
        {
            this.parameters.Add(node);
            base.VisitParameter(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            this.variableDeclarators.Add(node);
            base.VisitVariableDeclarator(node);
        }

        internal static bool IsLocalOrParameter(IdentifierNameSyntax identifier)
        {
            if (identifier == null)
            {
                return false;
            }

            if (identifier.Parent is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression is InstanceExpressionSyntax)
            {
                return false;
            }

            if (identifier.Identifier.ValueText == "value" &&
                identifier.FirstAncestor<AccessorDeclarationSyntax>() is AccessorDeclarationSyntax accessor &&
                accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
            {
                return true;
            }

            using (var walker = BorrowAndVisit(identifier.FirstAncestor<MemberDeclarationSyntax>(), () => new IdentifierTypeWalker()))
            {
                foreach (var parameter in walker.parameters)
                {
                    if (identifier.Identifier.ValueText == parameter.Identifier.ValueText)
                    {
                        return true;
                    }
                }

                foreach (var declarator in walker.variableDeclarators)
                {
                    if (identifier.Identifier.ValueText == declarator.Identifier.ValueText)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void Clear()
        {
            this.parameters.Clear();
            this.variableDeclarators.Clear();
        }
    }
}
