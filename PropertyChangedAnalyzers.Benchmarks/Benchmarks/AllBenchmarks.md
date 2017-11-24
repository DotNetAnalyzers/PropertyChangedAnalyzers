``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 7 SP1 (6.1.7601.0)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410126 Hz, Resolution=293.2443 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0


```
|                                   Method |           Mean |         Error |         StdDev |         Median |   Gen 0 | Allocated |
|----------------------------------------- |---------------:|--------------:|---------------:|---------------:|--------:|----------:|
|   INPC001ImplementINotifyPropertyChanged |   223,193.1 ns |  4,437.640 ns |  10,717.399 ns |   220,787.9 ns |  3.1738 |   21560 B |
| INPC002MutablePublicPropertyShouldNotify |    34,367.6 ns |    717.070 ns |   1,718.053 ns |    34,181.9 ns |  0.1221 |     960 B |
|         INPC003NotifyWhenPropertyChanges | 2,771,216.7 ns | 55,348.266 ns | 134,725.328 ns | 2,752,811.3 ns |       - |   29223 B |
|               INPC004UseCallerMemberName | 1,342,290.1 ns | 27,635.605 ns |  62,378.166 ns | 1,335,719.5 ns |       - |     544 B |
|   INPC005CheckIfDifferentBeforeNotifying | 1,289,471.3 ns | 25,742.803 ns |  53,163.353 ns | 1,270,596.8 ns |       - |     448 B |
|  INPC006UseObjectEqualsForReferenceTypes |       240.4 ns |      4.949 ns |      14.591 ns |       240.0 ns |  0.0699 |     440 B |
|                INPC006UseReferenceEquals |   655,551.2 ns | 14,818.027 ns |  42,753.395 ns |   652,556.1 ns |       - |     448 B |
|                    INPC007MissingInvoker |       216.7 ns |      4.313 ns |       6.455 ns |       214.9 ns |  0.0699 |     440 B |
|               INPC008StructMustNotNotify |       210.6 ns |      4.679 ns |       4.376 ns |       208.5 ns |  0.0699 |     440 B |
| INPC009DontRaiseChangeForMissingProperty | 2,557,204.3 ns | 28,636.828 ns |  23,913.046 ns | 2,555,890.4 ns | 31.2500 |  216386 B |
|             INPC010SetAndReturnSameField |    44,005.1 ns |    862.217 ns |   1,316.697 ns |    43,204.9 ns |  0.5493 |    3744 B |
|                        INPC011DontShadow |       212.3 ns |      4.161 ns |       4.086 ns |       209.7 ns |  0.0699 |     440 B |
|                 INPC012DontUseExpression | 1,742,134.4 ns | 43,131.672 ns |  47,940.735 ns | 1,715,166.4 ns |       - |     880 B |
|                         INPC013UseNameof | 1,839,511.3 ns | 35,514.798 ns |  43,615.361 ns | 1,826,182.8 ns |  5.8594 |   41792 B |
|   INPC014PreferSettingBackingFieldInCtor |   305,210.6 ns |  6,072.830 ns |   8,312.558 ns |   303,066.7 ns |       - |     444 B |
