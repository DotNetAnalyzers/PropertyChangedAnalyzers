﻿namespace PropertyChangedAnalyzers.Benchmarks;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using PropertyChangedAnalyzers.Benchmarks.Benchmarks;

public class BenchmarkTests
{
    private static IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers { get; } =
        typeof(KnownSymbol)
        .Assembly
        .GetTypes()
        .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t) && !t.IsAbstract)
        .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
        .ToArray();

    private static IReadOnlyList<Gu.Roslyn.Asserts.Benchmark> AllBenchmarks { get; } = AllAnalyzers
        .Select(x => Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, x))
        .ToArray();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        foreach (var benchmark in AllBenchmarks)
        {
            benchmark.Run();
        }
    }

    [TestCaseSource(nameof(AllBenchmarks))]
    public void Run(Gu.Roslyn.Asserts.Benchmark benchmark)
    {
        benchmark.Run();
    }
}
