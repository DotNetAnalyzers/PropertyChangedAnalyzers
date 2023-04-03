// ReSharper disable HeuristicUnreachableCode
#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable IDE0051 // Remove unused private members
namespace PropertyChangedAnalyzers.Benchmarks;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using PropertyChangedAnalyzers.Benchmarks.Benchmarks;

public static class Program
{
    public static void Main()
    {
        if (false)
        {
            var benchmark = Gu.Roslyn.Asserts.Benchmark.Create(
                Code.ValidCodeProject,
                new ClassDeclarationAnalyzer());

            // Warmup
            benchmark.Run();
            Console.WriteLine("Attach profiler and press any key to continue...");
            Console.ReadKey();
            benchmark.Run();
        }
        else
        {
            foreach (var summary in RunAll())
            {
                CopyResult(summary);
            }
        }
    }

    private static IEnumerable<Summary> RunAll() => new BenchmarkSwitcher(typeof(Program).Assembly).RunAll();

    private static IEnumerable<Summary> RunSingle<T>()
    {
        yield return BenchmarkRunner.Run<T>();
    }

    private static void CopyResult(Summary summary)
    {
        var name = summary.Title.Split('.').LastOrDefault()?.Split('-').FirstOrDefault();
        if (name is null)
        {
            Console.WriteLine("Did not find name in: " + summary.Title);
            Console.WriteLine("Press any key to exit.");
            _ = Console.ReadKey();
            return;
        }

        var pattern = $"{summary.Title.Split('-').First()}-report-github.md";
        var sourceFileName = Directory.EnumerateFiles(summary.ResultsDirectoryPath, pattern)
                                      .SingleOrDefault();
        if (sourceFileName is null)
        {
            Console.WriteLine("Did not find a file matching the pattern: " + pattern);
            Console.WriteLine("Press any key to exit.");
            _ = Console.ReadKey();
            return;
        }

        var destinationFileName = Path.ChangeExtension(FindCsFile(), ".md");
        Console.WriteLine($"Copy:");
        Console.WriteLine($"Source: {sourceFileName}");
        Console.WriteLine($"Target: {destinationFileName}");
        File.Copy(sourceFileName, destinationFileName, overwrite: true);

        string FindCsFile()
        {
            return Directory.EnumerateFiles(
                                AppDomain.CurrentDomain.BaseDirectory.Split(new[] { "\\bin\\" }, StringSplitOptions.RemoveEmptyEntries).First(),
                                $"{name}.cs",
                                SearchOption.AllDirectories)
                            .Single();
        }
    }
}
