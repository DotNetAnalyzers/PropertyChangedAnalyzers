``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical cores and 8 physical cores
Frequency=3410117 Hz, Resolution=293.2451 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                                 Method |           Mean |          Error |         StdDev |   Gen 0 |  Gen 1 | Allocated |
|--------------------------------------- |---------------:|---------------:|---------------:|--------:|-------:|----------:|
| INPC001ImplementINotifyPropertyChanged |   219,313.2 ns |   4,361.063 ns |   9,754.142 ns |  3.4180 |      - |   21978 B |
|       INPC003NotifyWhenPropertyChanges | 2,778,540.6 ns |  55,431.387 ns | 114,475.426 ns |       - |      - |   29874 B |
|             INPC004UseCallerMemberName |   298,644.7 ns |   5,781.347 ns |   6,657.807 ns |       - |      - |     540 B |
| INPC005CheckIfDifferentBeforeNotifying | 1,305,443.8 ns |  25,077.323 ns |  27,873.375 ns |       - |      - |     448 B |
|                  INPC007MissingInvoker |       219.6 ns |       4.383 ns |       8.954 ns |  0.0699 |      - |     440 B |
|             INPC008StructMustNotNotify |       224.6 ns |       4.517 ns |      10.558 ns |  0.0699 |      - |     440 B |
|                      INPC011DontShadow |       225.3 ns |       4.481 ns |       6.976 ns |  0.0699 |      - |     440 B |
| INPC014PreferSettingBackingFieldInCtor |   321,003.9 ns |   6,392.042 ns |  10,138.455 ns |       - |      - |     444 B |
|                       ArgumentAnalyzer | 7,494,574.6 ns | 182,653.385 ns | 535,690.760 ns | 46.8750 |      - |  342838 B |
|                    IfStatementAnalyzer |   635,585.2 ns |  12,652.447 ns |  31,508.983 ns |       - |      - |     448 B |
|                     InvocationAnalyzer | 1,064,360.8 ns |   2,211.127 ns |   1,846.391 ns |       - |      - |    1248 B |
|            PropertyDeclarationAnalyzer |    30,661.2 ns |     601.887 ns |     533.557 ns |  0.7935 | 0.0610 |    5203 B |
