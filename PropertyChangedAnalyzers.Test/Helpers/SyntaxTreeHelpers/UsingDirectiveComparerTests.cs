namespace PropertyChangedAnalyzers.Test.Helpers.SyntaxTreeHelpers
{
    using System;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class UsingDirectiveComparerTests
    {
        [TestCase("System", "System", 0)]
        [TestCase("System", "System.Collections", -1)]
        [TestCase("A", "B", -1)]
        public void Simple(string s1, string s2, int expected)
        {
            var ud1 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(s1));
            var ud2 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(s2));
            Assert.AreEqual(expected, UsingDirectiveComparer.Compare(ud1, ud2));
            Assert.AreEqual(-expected, UsingDirectiveComparer.Compare(ud2, ud1));
            Assert.AreEqual(expected, Math.Sign(StringComparer.OrdinalIgnoreCase.Compare(s1, s2)));
        }

        [TestCase("System", "B", -1)]
        public void SystemFirst(string s1, string s2, int expected)
        {
            var ud1 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(s1));
            var ud2 = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(s2));
            Assert.AreEqual(expected, UsingDirectiveComparer.Compare(ud1, ud2));
            Assert.AreEqual(-expected, UsingDirectiveComparer.Compare(ud2, ud1));
        }
    }
}