``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical and 8 physical cores
.NET Core SDK=3.0.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT
  DefaultJob : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT


```
|                      Method |        Mean |      Error |      StdDev |      Median | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------- |------------:|-----------:|------------:|------------:|------:|------:|------:|----------:|
|            ArgumentAnalyzer |   787.21 us |  59.172 us |   162.98 us |   736.40 us |     - |     - |     - |   13536 B |
|          AssignmentAnalyzer |    67.49 us |   7.037 us |    19.50 us |    62.20 us |     - |     - |     - |     440 B |
|    ClassDeclarationAnalyzer | 4,869.85 us | 646.941 us | 1,907.52 us | 4,399.30 us |     - |     - |     - |  211088 B |
|            EqualityAnalyzer |   532.25 us |  69.602 us |   195.17 us |   473.30 us |     - |     - |     - |    4960 B |
|               EventAnalyzer |    40.76 us |   4.914 us |    14.10 us |    38.70 us |     - |     - |     - |     440 B |
|          InvocationAnalyzer |   606.42 us |  65.126 us |   184.75 us |   547.90 us |     - |     - |     - |   10680 B |
|   MethodDeclarationAnalyzer |   202.61 us |  29.674 us |    84.18 us |   165.60 us |     - |     - |     - |    2992 B |
|            MutationAnalyzer |   357.08 us |  28.319 us |    76.56 us |   329.50 us |     - |     - |     - |     440 B |
| PropertyDeclarationAnalyzer | 1,863.44 us | 219.753 us |   608.94 us | 1,684.60 us |     - |     - |     - |   48728 B |
|         SetAccessorAnalyzer |    58.93 us |   7.175 us |    20.36 us |    57.40 us |     - |     - |     - |     440 B |
|              StructAnalyzer |    29.07 us |   7.347 us |    21.20 us |    18.05 us |     - |     - |     - |     440 B |
