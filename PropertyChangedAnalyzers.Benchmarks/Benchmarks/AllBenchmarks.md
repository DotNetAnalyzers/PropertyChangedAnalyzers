``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 7 SP1 (6.1.7601.0)
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical cores and 8 physical cores
Frequency=3410087 Hz, Resolution=293.2477 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                                 Method |           Mean |          Error |         StdDev |         Median |   Gen 0 |  Gen 1 | Allocated |
|--------------------------------------- |---------------:|---------------:|---------------:|---------------:|--------:|-------:|----------:|
| INPC001ImplementINotifyPropertyChanged |   218,477.0 ns |   4,324.154 ns |   4,979.702 ns |   217,967.3 ns |  3.4180 |      - |   22026 B |
|       INPC003NotifyWhenPropertyChanges | 2,631,209.6 ns |  51,928.661 ns |  93,638.088 ns | 2,594,859.0 ns |       - |      - |   29881 B |
|             INPC004UseCallerMemberName |   299,446.3 ns |   5,889.234 ns |   6,782.050 ns |   298,723.8 ns |       - |      - |     540 B |
|                  INPC007MissingInvoker |       214.3 ns |       4.280 ns |       7.827 ns |       213.1 ns |  0.0699 |      - |     440 B |
|             INPC008StructMustNotNotify |       217.2 ns |       4.163 ns |       5.113 ns |       217.6 ns |  0.0699 |      - |     440 B |
|                      INPC011DontShadow |       224.1 ns |       4.501 ns |       8.116 ns |       224.1 ns |  0.0699 |      - |     440 B |
| INPC014PreferSettingBackingFieldInCtor |   316,963.9 ns |   6,259.119 ns |   9,927.626 ns |   315,165.5 ns |       - |      - |     444 B |
|                       ArgumentAnalyzer | 7,844,487.9 ns | 262,584.973 ns | 770,116.271 ns | 7,546,467.3 ns | 46.8750 |      - |  344938 B |
|                    IfStatementAnalyzer |   975,980.1 ns |   8,632.159 ns |  18,395.834 ns |   976,249.4 ns |       - |      - |     448 B |
|                     InvocationAnalyzer | 1,126,546.5 ns |  19,670.516 ns |  17,437.399 ns | 1,118,427.9 ns |       - |      - |    2224 B |
|            PropertyDeclarationAnalyzer |    30,515.1 ns |     605.425 ns |   1,594.931 ns |    29,699.7 ns |  0.7324 | 0.0610 |    4886 B |
