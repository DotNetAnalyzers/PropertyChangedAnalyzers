``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical cores and 8 physical cores
Frequency=3410117 Hz, Resolution=293.2451 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                                 Method |           Mean |         Error |         StdDev |  Gen 0 |  Gen 1 | Allocated |
|--------------------------------------- |---------------:|--------------:|---------------:|-------:|-------:|----------:|
| INPC001ImplementINotifyPropertyChanged |   275,481.6 ns |  5,468.795 ns |  11,888.710 ns | 4.3945 |      - |   28748 B |
|       INPC003NotifyWhenPropertyChanges | 2,965,517.2 ns | 58,726.915 ns | 119,963.525 ns |      - |      - |   30262 B |
|             INPC004UseCallerMemberName |   376,207.9 ns |  7,505.416 ns |  11,001.342 ns |      - |      - |     540 B |
|                  INPC007MissingInvoker |       207.8 ns |      1.308 ns |       1.223 ns | 0.0699 |      - |     440 B |
|             INPC008StructMustNotNotify |       208.1 ns |      3.357 ns |       3.140 ns | 0.0699 |      - |     440 B |
|                      INPC011DontShadow |       211.4 ns |      4.096 ns |       4.207 ns | 0.0699 |      - |     440 B |
| INPC014PreferSettingBackingFieldInCtor |   319,334.7 ns |  6,329.080 ns |   9,076.978 ns |      - |      - |     444 B |
|                       ArgumentAnalyzer | 2,355,530.2 ns | 55,451.992 ns |  92,647.822 ns |      - |      - |    3200 B |
|                    IfStatementAnalyzer |   674,151.5 ns | 14,647.281 ns |  21,469.794 ns |      - |      - |     448 B |
|                     InvocationAnalyzer | 1,178,161.5 ns | 23,404.468 ns |  25,042.516 ns |      - |      - |    2320 B |
|            PropertyDeclarationAnalyzer |    30,274.1 ns |    590.690 ns |     632.032 ns | 0.7629 | 0.0610 |    4958 B |
