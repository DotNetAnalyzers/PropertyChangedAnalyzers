``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical cores and 8 physical cores
Frequency=3410117 Hz, Resolution=293.2451 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                                 Method |           Mean |         Error |        StdDev |  Gen 0 |  Gen 1 | Allocated |
|--------------------------------------- |---------------:|--------------:|--------------:|-------:|-------:|----------:|
| INPC001ImplementINotifyPropertyChanged |   305,285.6 ns |  9,045.350 ns |  26,242.20 ns | 4.3945 |      - |   29332 B |
|       INPC003NotifyWhenPropertyChanges | 2,960,725.1 ns | 67,987.062 ns | 198,321.45 ns |      - |      - |   16960 B |
|             INPC004UseCallerMemberName |   413,066.8 ns |  8,216.627 ns |  18,377.66 ns |      - |      - |     540 B |
|                  INPC007MissingInvoker |       237.3 ns |      5.101 ns |      14.55 ns | 0.0699 |      - |     440 B |
|             INPC008StructMustNotNotify |       243.6 ns |      6.158 ns |      17.67 ns | 0.0696 |      - |     440 B |
|                      INPC011DontShadow |       246.3 ns |      6.209 ns |      17.71 ns | 0.0699 |      - |     440 B |
| INPC014PreferSettingBackingFieldInCtor |   363,309.6 ns | 10,916.810 ns |  31,844.85 ns |      - |      - |     448 B |
|                       ArgumentAnalyzer | 2,836,870.2 ns | 60,674.265 ns | 170,137.14 ns |      - |      - |    3840 B |
|                    IfStatementAnalyzer |   784,088.1 ns | 20,239.603 ns |  58,395.88 ns |      - |      - |     448 B |
|                     InvocationAnalyzer | 1,388,388.4 ns | 34,641.430 ns |  99,392.76 ns |      - |      - |    2704 B |
|            PropertyDeclarationAnalyzer |    34,104.6 ns |    806.640 ns |   2,340.21 ns | 0.7629 | 0.0610 |    4958 B |
