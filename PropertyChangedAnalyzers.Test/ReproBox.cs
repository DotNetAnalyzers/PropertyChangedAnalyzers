namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using PropertyChangedAnalyzers;

    [Ignore("For harvesting test cases only.")]
    public static class ReproBox
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers =
            typeof(KnownSymbol).Assembly.GetTypes()
                               .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                               .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                               .ToArray();

        private static readonly Solution Solution = CodeFactory.CreateSolution(
            new FileInfo("C:\\Git\\Gu.Reactive\\Gu.Reactive.sln"),
            AllAnalyzers,
            MetadataReferences.FromAttributes());

        [Ignore("For harvesting test cases only.")]
        [TestCaseSource(nameof(AllAnalyzers))]
        public static void SolutionRepro(DiagnosticAnalyzer analyzer)
        {
            Assert.Inconclusive("VS does not understand [Explicit]");
            RoslynAssert.Valid(analyzer, Solution);
        }

        [Ignore("For harvesting test cases only.")]
        [TestCaseSource(nameof(AllAnalyzers))]
        public static void Repro(DiagnosticAnalyzer analyzer)
        {
            Assert.Inconclusive("VS does not understand [Explicit]");
            var code = @"
namespace N
{
    public class C
    {
    }
}";
            RoslynAssert.Valid(analyzer, code);
        }
    }
}
