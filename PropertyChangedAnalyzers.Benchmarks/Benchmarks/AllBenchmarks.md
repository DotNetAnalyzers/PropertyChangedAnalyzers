``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical cores and 8 physical cores
Frequency=3410107 Hz, Resolution=293.2459 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                                 Method |           Mean |         Error |        StdDev |  Gen 0 |  Gen 1 | Allocated |
|--------------------------------------- |---------------:|--------------:|--------------:|-------:|-------:|----------:|
| INPC001ImplementINotifyPropertyChanged |   245,591.5 ns |  8,474.101 ns |  24,986.08 ns | 3.9063 |      - |   26716 B |
|       INPC003NotifyWhenPropertyChanges | 3,312,840.9 ns | 79,998.881 ns | 232,091.26 ns |      - |      - |   20192 B |
|             INPC004UseCallerMemberName |   509,981.4 ns | 11,763.670 ns |  33,562.41 ns |      - |      - |     544 B |
|                  INPC007MissingInvoker |       261.4 ns |      6.402 ns |      18.68 ns | 0.0696 |      - |     440 B |
|             INPC008StructMustNotNotify |       234.2 ns |      5.035 ns |      14.69 ns | 0.0699 |      - |     440 B |
|                      INPC011DontShadow |       240.2 ns |      6.490 ns |      19.03 ns | 0.0696 |      - |     440 B |
| INPC014PreferSettingBackingFieldInCtor |   422,760.3 ns |  8,420.518 ns |  21,432.91 ns |      - |      - |     444 B |
|                       ArgumentAnalyzer | 2,825,995.3 ns | 58,931.871 ns | 171,907.03 ns |      - |      - |    3584 B |
|                    IfStatementAnalyzer |   818,734.4 ns | 21,640.615 ns |  63,468.18 ns |      - |      - |     448 B |
|                     InvocationAnalyzer | 1,592,338.3 ns | 37,476.547 ns | 109,912.23 ns |      - |      - |    2624 B |
|            PropertyDeclarationAnalyzer |    33,480.3 ns |    801.377 ns |   2,260.30 ns | 0.3052 | 0.0610 |    2390 B |
