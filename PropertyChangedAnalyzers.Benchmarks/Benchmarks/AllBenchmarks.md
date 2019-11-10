``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical and 8 physical cores
.NET Core SDK=3.0.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT
  DefaultJob : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT


```
|                      Method |        Mean |      Error |    StdDev |      Median | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------- |------------:|-----------:|----------:|------------:|------:|------:|------:|----------:|
|            ArgumentAnalyzer |   797.52 us |  80.964 us | 229.68 us |   714.70 us |     - |     - |     - |   13696 B |
|          AssignmentAnalyzer |    70.78 us |   7.783 us |  21.57 us |    65.20 us |     - |     - |     - |     440 B |
|    ClassDeclarationAnalyzer | 3,609.13 us | 301.632 us | 850.76 us | 3,492.00 us |     - |     - |     - |  211304 B |
|            EqualityAnalyzer |   499.32 us |  50.271 us | 143.43 us |   446.00 us |     - |     - |     - |    5088 B |
|               EventAnalyzer |    39.50 us |   4.113 us |  11.93 us |    38.30 us |     - |     - |     - |     440 B |
|          InvocationAnalyzer |   605.67 us |  69.551 us | 199.55 us |   547.20 us |     - |     - |     - |   10888 B |
|   MethodDeclarationAnalyzer |   190.03 us |  28.013 us |  78.55 us |   155.50 us |     - |     - |     - |    3032 B |
|            MutationAnalyzer |   396.62 us |  33.535 us |  95.13 us |   380.20 us |     - |     - |     - |     440 B |
| PropertyDeclarationAnalyzer | 1,578.04 us | 144.880 us | 418.01 us | 1,447.80 us |     - |     - |     - |   49152 B |
|         SetAccessorAnalyzer |    53.82 us |   5.902 us |  16.84 us |    51.85 us |     - |     - |     - |     440 B |
|              StructAnalyzer |    33.66 us |   7.390 us |  21.56 us |    20.50 us |     - |     - |     - |     440 B |
