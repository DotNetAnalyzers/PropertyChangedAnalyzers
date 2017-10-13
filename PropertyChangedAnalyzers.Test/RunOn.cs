namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class RunOn
    {
        private static readonly IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers = typeof(AnalyzerConstants)
            .Assembly
            .GetTypes()
            .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
            .Select(t => (DiagnosticAnalyzer) Activator.CreateInstance(t))
            .ToArray();

        [TestCaseSource(nameof(AllAnalyzers))]
        public void PropertyChangedAnalyzersSln(DiagnosticAnalyzer analyzer)
        {
            AnalyzerAssert.Valid(analyzer, CodeFactory.FindSolutionFile("PropertyChangedAnalyzers.sln"));
        }
    }
}