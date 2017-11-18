``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410107 Hz, Resolution=293.2459 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                                   Method |           Mean |         Error |        StdDev |   Gen 0 | Allocated |
 |----------------------------------------- |---------------:|--------------:|--------------:|--------:|----------:|
 |   INPC001ImplementINotifyPropertyChanged |   192,182.8 ns |  3,838.493 ns |   5,626.41 ns |  2.6855 |   17456 B |
 | INPC002MutablePublicPropertyShouldNotify |    34,360.9 ns |    842.502 ns |   2,470.91 ns |  0.0610 |     720 B |
 |         INPC003NotifyWhenPropertyChanges | 2,756,935.2 ns | 57,471.554 ns | 169,456.17 ns |       - |   28522 B |
 |               INPC004UseCallerMemberName | 1,368,509.7 ns | 27,282.923 ns |  78,279.82 ns |       - |     544 B |
 |   INPC005CheckIfDifferentBeforeNotifying | 1,294,174.9 ns | 31,475.452 ns |  91,815.37 ns |       - |     448 B |
 |  INPC006UseObjectEqualsForReferenceTypes |       236.1 ns |      4.904 ns |      12.83 ns |  0.0696 |     440 B |
 |                INPC006UseReferenceEquals |   622,634.9 ns | 12,751.807 ns |  36,381.62 ns |       - |     448 B |
 |                    INPC007MissingInvoker |       237.0 ns |      5.680 ns |      16.48 ns |  0.0696 |     440 B |
 |               INPC008StructMustNotNotify |       236.9 ns |      6.440 ns |      18.89 ns |  0.0696 |     440 B |
 | INPC009DontRaiseChangeForMissingProperty | 2,706,104.8 ns | 85,773.581 ns | 251,559.06 ns | 27.3438 |  202754 B |
 |             INPC010SetAndReturnSameField |    43,775.8 ns |  1,007.977 ns |   2,956.22 ns |  0.5493 |    3616 B |
 |                        INPC011DontShadow |       227.2 ns |      4.523 ns |      12.46 ns |  0.0696 |     440 B |
 |                 INPC012DontUseExpression | 1,838,084.4 ns | 36,361.599 ns |  90,553.00 ns |       - |     880 B |
 |                         INPC013UseNameof | 1,833,289.9 ns | 36,425.745 ns |  99,098.93 ns |  3.9063 |   39584 B |
 |   INPC014PreferSettingBackingFieldInCtor |   319,730.0 ns |  7,748.412 ns |  22,724.75 ns |       - |     444 B |
