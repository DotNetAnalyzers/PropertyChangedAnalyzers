``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410097 Hz, Resolution=293.2468 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                                   Method |           Mean |          Error |          StdDev |   Gen 0 | Allocated |
 |----------------------------------------- |---------------:|---------------:|----------------:|--------:|----------:|
 |   INPC001ImplementINotifyPropertyChanged |   184,878.9 ns |  3,622.3352 ns |   4,835.7134 ns |  2.4414 |   17232 B |
 | INPC002MutablePublicPropertyShouldNotify |    30,048.7 ns |    206.5471 ns |     161.2583 ns |  0.0916 |     720 B |
 |         INPC003NotifyWhenPropertyChanges | 2,545,915.4 ns | 49,862.9594 ns | 100,725.6371 ns |       - |   27587 B |
 |               INPC004UseCallerMemberName | 1,128,964.4 ns | 18,068.8381 ns |  15,088.2967 ns |       - |     544 B |
 |   INPC005CheckIfDifferentBeforeNotifying | 1,097,833.8 ns | 21,148.5921 ns |  27,499.1435 ns |       - |     448 B |
 |  INPC006UseObjectEqualsForReferenceTypes |       215.6 ns |      4.2410 ns |       6.3478 ns |  0.0699 |     440 B |
 |                INPC006UseReferenceEquals |   518,316.5 ns |  1,874.9473 ns |   1,753.8267 ns |       - |     448 B |
 |                    INPC007MissingInvoker |       214.2 ns |      4.3151 ns |       6.4587 ns |  0.0699 |     440 B |
 |               INPC008StructMustNotNotify |       206.8 ns |      0.6068 ns |       0.5379 ns |  0.0699 |     440 B |
 | INPC009DontRaiseChangeForMissingProperty | 2,114,535.1 ns | 12,073.7252 ns |  10,082.1064 ns | 27.3438 |  189570 B |
 |             INPC010SetAndReturnSameField |    39,054.0 ns |    250.2095 ns |     208.9362 ns |  0.4883 |    3496 B |
 |                        INPC011DontShadow |       209.8 ns |      1.1952 ns |       0.9331 ns |  0.0699 |     440 B |
 |                 INPC012DontUseExpression | 1,613,794.0 ns | 31,793.7714 ns |  45,597.6769 ns |       - |     880 B |
 |                         INPC013UseNameof | 1,634,184.4 ns | 45,259.4455 ns |  46,478.1139 ns |  3.9063 |   38096 B |
