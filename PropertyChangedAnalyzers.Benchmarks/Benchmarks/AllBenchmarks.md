``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical cores and 8 physical cores
Frequency=3410087 Hz, Resolution=293.2477 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                                   Method |           Mean |          Error |        StdDev |         Median |   Gen 0 |  Gen 1 | Allocated |
|----------------------------------------- |---------------:|---------------:|--------------:|---------------:|--------:|-------:|----------:|
|   INPC001ImplementINotifyPropertyChanged |   238,893.5 ns |   7,330.146 ns |  21,498.05 ns |   239,562.2 ns |  2.9297 |      - |   21843 B |
|         INPC003NotifyWhenPropertyChanges | 4,169,635.5 ns |   5,405.342 ns |   3,908.42 ns | 4,168,176.3 ns |       - |      - |   29872 B |
|               INPC004UseCallerMemberName |   343,969.8 ns |   7,893.070 ns |  23,149.01 ns |   343,503.8 ns |       - |      - |     540 B |
|   INPC005CheckIfDifferentBeforeNotifying | 1,386,672.2 ns |  42,811.765 ns | 124,883.93 ns | 1,363,822.2 ns |       - |      - |     448 B |
|                    INPC007MissingInvoker |       242.3 ns |       7.053 ns |      19.78 ns |       239.5 ns |  0.0696 |      - |     440 B |
|               INPC008StructMustNotNotify |       239.4 ns |       5.559 ns |      15.86 ns |       239.8 ns |  0.0699 |      - |     440 B |
| INPC009DontRaiseChangeForMissingProperty | 3,193,852.0 ns | 116,186.215 ns | 342,577.68 ns | 3,185,674.3 ns | 35.1563 |      - |  238364 B |
|                        INPC011DontShadow |       246.4 ns |       5.155 ns |      14.79 ns |       244.5 ns |  0.0696 |      - |     440 B |
|   INPC014PreferSettingBackingFieldInCtor |   348,801.5 ns |   7,911.361 ns |  23,077.81 ns |   347,530.7 ns |       - |      - |     444 B |
|                         ArgumentAnalyzer | 7,205,222.1 ns | 182,179.067 ns | 534,299.67 ns | 7,154,332.5 ns | 23.4375 |      - |  196084 B |
|                      IfStatementAnalyzer |   641,743.7 ns |  16,561.389 ns |  48,571.69 ns |   637,364.1 ns |       - |      - |     448 B |
|              PropertyDeclarationAnalyzer |    43,080.4 ns |   2,453.004 ns |   7,232.74 ns |    48,890.6 ns |  0.7935 | 0.0610 |    5203 B |
