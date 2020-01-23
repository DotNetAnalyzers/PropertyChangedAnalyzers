``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical and 8 physical cores
.NET Core SDK=3.1.101
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT
  DefaultJob : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT


```
|                      Method |        Mean |      Error |    StdDev |      Median | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------- |------------:|-----------:|----------:|------------:|------:|------:|------:|----------:|
|            ArgumentAnalyzer |   786.93 us |  80.096 us | 233.64 us |   755.30 us |     - |     - |     - |    8512 B |
|          AssignmentAnalyzer |    65.59 us |   6.335 us |  17.87 us |    61.85 us |     - |     - |     - |     440 B |
|    ClassDeclarationAnalyzer | 3,564.37 us | 264.862 us | 759.94 us | 3,381.50 us |     - |     - |     - |  214544 B |
|               EventAnalyzer |    45.19 us |   5.190 us |  14.98 us |    43.65 us |     - |     - |     - |    2024 B |
|          InvocationAnalyzer |   149.92 us |  13.192 us |  35.21 us |   142.00 us |     - |     - |     - |     696 B |
|   MethodDeclarationAnalyzer |   198.53 us |  28.005 us |  78.99 us |   163.20 us |     - |     - |     - |    1216 B |
|            MutationAnalyzer |   377.61 us |  33.536 us |  94.59 us |   347.40 us |     - |     - |     - |     440 B |
| PropertyDeclarationAnalyzer |   942.49 us |  69.684 us | 201.05 us |   909.50 us |     - |     - |     - |   30784 B |
|         SetAccessorAnalyzer |   173.75 us |  16.660 us |  45.89 us |   154.75 us |     - |     - |     - |    1416 B |
|              StructAnalyzer |    32.22 us |   8.016 us |  23.38 us |    18.00 us |     - |     - |     - |     440 B |
