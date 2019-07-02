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

    [Explicit("For harvesting test cases only.")]
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
            RoslynAssert.MetadataReferences);

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void SolutionRepro(DiagnosticAnalyzer analyzer)
        {
            RoslynAssert.Valid(analyzer, Solution);
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public static void Repro(DiagnosticAnalyzer analyzer)
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
    }
}";
            RoslynAssert.Valid(analyzer, testCode);
        }
    }
}
