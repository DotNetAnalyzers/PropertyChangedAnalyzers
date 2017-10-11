﻿namespace PropertyChangedAnalyzers.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Benchmarks.Benchmarks;

    internal class BenchmarkWalkerTests
    {
        private static IReadOnlyList<DiagnosticAnalyzer> AllAnalyzers { get; } = typeof(KnownSymbol).Assembly
                                                                                                    .GetTypes()
                                                                                                    .Where(typeof(DiagnosticAnalyzer).IsAssignableFrom)
                                                                                                    .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                                                                                                    .ToArray();

        private static IReadOnlyList<Type> AllBenchmarkTypes { get; } = typeof(AnalyzerBenchmarks).Assembly.GetTypes()
                                                                                                  .Where(typeof(AnalyzerBenchmarks).IsAssignableFrom)
                                                                                                  .ToArray();

        private static IReadOnlyList<BenchmarkWalker> AllBenchmarkWalkers { get; } = AllAnalyzers
            .Select(x => new BenchmarkWalker(Code.AnalyzersProject, x))
            .ToArray();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            foreach (var walker in AllBenchmarkWalkers)
            {
                walker.Run();
            }
        }

        [TestCaseSource(nameof(AllBenchmarkWalkers))]
        public void Run(BenchmarkWalker walker)
        {
            walker.Run();
        }

        [TestCaseSource(nameof(AllAnalyzers))]
        public void AllAnalyzersHaveBenchmarks(DiagnosticAnalyzer analyzer)
        {
            var id = analyzer.SupportedDiagnostics.Single().Id;
            var expectedName = id + (id.Contains("_") ? "_" : string.Empty) + "Benchmarks";
            var match = AllBenchmarkTypes.SingleOrDefault(x => x.Name == expectedName);
            Assert.NotNull(match, expectedName);
        }

        [Test]
        public void ProjectFileExists()
        {
            var projectFile = Path.Combine(Program.ProjectDirectory, "PropertyChangedAnalyzers.Benchmarks.csproj");
            Assert.AreEqual(true, File.Exists(projectFile), projectFile);
        }

        [Test]
        public void BenchmarksDirectoryExists()
        {
            Assert.AreEqual(true, Directory.Exists(Program.BenchmarksDirectory), Program.BenchmarksDirectory);
        }
    }
}