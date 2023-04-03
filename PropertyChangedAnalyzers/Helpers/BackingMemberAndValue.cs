namespace PropertyChangedAnalyzers;

using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[DebuggerDisplay("Member: {this.Member.GetText().ToString()} Parameter:{this.Parameter.GetText().ToString()},nq")]
internal readonly struct BackingMemberAndValue
{
    internal readonly ExpressionSyntax Member;

    internal readonly IdentifierNameSyntax Parameter;

    internal BackingMemberAndValue(ExpressionSyntax member, IdentifierNameSyntax parameter)
    {
        this.Member = member;
        this.Parameter = parameter;
    }
}
