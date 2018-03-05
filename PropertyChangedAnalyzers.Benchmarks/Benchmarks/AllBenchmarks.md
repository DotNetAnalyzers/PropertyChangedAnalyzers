``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical cores and 8 physical cores
Frequency=3410087 Hz, Resolution=293.2477 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                                 Method |           Mean |           Error |          StdDev |         Median |   Gen 0 |  Gen 1 | Allocated |
|--------------------------------------- |---------------:|----------------:|----------------:|---------------:|--------:|-------:|----------:|
| INPC001ImplementINotifyPropertyChanged |   311,891.5 ns |   9,768.6048 ns |  28,649.6269 ns |   310,244.6 ns |  4.3945 |      - |   28748 B |
|       INPC003NotifyWhenPropertyChanges | 3,734,101.8 ns | 208,670.4805 ns | 615,269.6197 ns | 3,521,620.5 ns |       - |      - |   30070 B |
|             INPC004UseCallerMemberName |   408,991.2 ns |   9,724.7468 ns |  27,902.1217 ns |   409,926.7 ns |       - |      - |     540 B |
|                  INPC007MissingInvoker |       260.2 ns |       8.4639 ns |      24.5552 ns |       261.3 ns |  0.0696 |      - |     440 B |
|             INPC008StructMustNotNotify |       359.6 ns |       0.3593 ns |       0.2377 ns |       359.5 ns |  0.0699 |      - |     440 B |
|                      INPC011DontShadow |       240.9 ns |       6.9438 ns |      20.4740 ns |       241.2 ns |  0.0699 |      - |     440 B |
| INPC014PreferSettingBackingFieldInCtor |   367,947.1 ns |  10,146.7161 ns |  29,758.5618 ns |   366,897.0 ns |       - |      - |     444 B |
|                       ArgumentAnalyzer | 7,116,075.5 ns | 250,500.8566 ns | 738,607.4277 ns | 7,075,075.2 ns | 23.4375 |      - |  170229 B |
|                    IfStatementAnalyzer |   777,693.6 ns |  21,504.2387 ns |  63,068.2095 ns |   767,919.2 ns |       - |      - |     448 B |
|                     InvocationAnalyzer | 1,360,934.1 ns |  39,253.9898 ns | 115,125.1570 ns | 1,358,831.1 ns |       - |      - |    2320 B |
|            PropertyDeclarationAnalyzer |    36,393.1 ns |   1,026.5145 ns |   3,026.7011 ns |    36,146.1 ns |  0.7324 | 0.0610 |    4958 B |
