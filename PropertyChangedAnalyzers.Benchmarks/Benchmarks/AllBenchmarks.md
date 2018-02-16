``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 7 SP1 (6.1.7601.0)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410107 Hz, Resolution=293.2459 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                                   Method |           Mean |         Error |        StdDev |   Gen 0 | Allocated |
|----------------------------------------- |---------------:|--------------:|--------------:|--------:|----------:|
|   INPC001ImplementINotifyPropertyChanged |   265,932.6 ns | 16,380.858 ns |  48,299.33 ns |  3.1738 |   21560 B |
| INPC002MutablePublicPropertyShouldNotify |    36,342.7 ns |  1,014.855 ns |   2,960.38 ns |  0.1221 |     960 B |
|         INPC003NotifyWhenPropertyChanges | 3,001,409.2 ns | 66,535.303 ns | 191,969.56 ns |       - |   29294 B |
|               INPC004UseCallerMemberName | 1,423,406.0 ns | 34,841.289 ns | 102,183.47 ns |       - |     544 B |
|   INPC005CheckIfDifferentBeforeNotifying | 1,421,906.6 ns | 34,579.949 ns | 101,959.76 ns |       - |     448 B |
|  INPC006UseObjectEqualsForReferenceTypes |       236.9 ns |      6.420 ns |      18.93 ns |  0.0699 |     440 B |
|                INPC006UseReferenceEquals |   673,814.3 ns | 13,386.842 ns |  37,094.87 ns |       - |     448 B |
|                    INPC007MissingInvoker |       243.1 ns |      4.882 ns |      11.41 ns |  0.0696 |     440 B |
|               INPC008StructMustNotNotify |       240.9 ns |      5.551 ns |      16.37 ns |  0.0696 |     440 B |
| INPC009DontRaiseChangeForMissingProperty | 2,841,454.8 ns | 64,961.190 ns | 188,464.19 ns | 31.2500 |  218211 B |
|             INPC010SetAndReturnSameField |    49,058.8 ns |  1,096.588 ns |   3,181.40 ns |  0.5493 |    3744 B |
|                        INPC011DontShadow |       237.2 ns |      7.517 ns |      22.16 ns |  0.0696 |     440 B |
|                 INPC012DontUseExpression | 2,122,602.7 ns | 65,073.181 ns | 190,848.38 ns |       - |     880 B |
|                         INPC013UseNameof | 2,097,013.7 ns | 55,446.276 ns | 162,614.33 ns |  3.9063 |   41888 B |
|   INPC014PreferSettingBackingFieldInCtor |   349,553.1 ns |  8,530.822 ns |  25,153.32 ns |       - |     444 B |
