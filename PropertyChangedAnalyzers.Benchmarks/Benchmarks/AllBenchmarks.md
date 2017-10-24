``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410117 Hz, Resolution=293.2451 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                                   Method |            Mean |          Error |         StdDev |   Gen 0 | Allocated |
 |----------------------------------------- |----------------:|---------------:|---------------:|--------:|----------:|
 |   INPC001ImplementINotifyPropertyChanged |   291,718.45 ns |    819.7700 ns |     684.545 ns |  2.4414 |   16696 B |
 | INPC002MutablePublicPropertyShouldNotify |    33,984.33 ns |    441.3522 ns |     262.642 ns |       - |     320 B |
 |         INPC003NotifyWhenPropertyChanges | 2,994,378.59 ns | 78,226.7768 ns | 229,425.595 ns |  3.9063 |   31392 B |
 |               INPC004UseCallerMemberName | 1,324,601.45 ns | 34,568.4879 ns | 100,289.451 ns |       - |     480 B |
 |   INPC005CheckIfDifferentBeforeNotifying | 1,182,417.06 ns | 32,759.3991 ns |  95,040.957 ns |       - |      48 B |
 |  INPC006UseObjectEqualsForReferenceTypes |   631,682.40 ns | 16,057.0222 ns |  46,584.333 ns |       - |      48 B |
 |                INPC006UseReferenceEquals |   659,423.12 ns | 15,543.8256 ns |  44,598.149 ns |       - |      48 B |
 |                    INPC007MissingInvoker |        35.59 ns |      1.0138 ns |       2.941 ns |  0.0063 |      40 B |
 |               INPC008StructMustNotNotify |        35.78 ns |      1.2211 ns |       3.581 ns |  0.0063 |      40 B |
 | INPC009DontRaiseChangeForMissingProperty | 2,815,445.31 ns | 77,335.1667 ns | 225,590.311 ns | 27.3438 |  187074 B |
 |             INPC010SetAndReturnSameField |    48,591.48 ns |  1,108.8551 ns |   3,216.990 ns |  0.4272 |    3096 B |
 |                        INPC011DontShadow |        35.59 ns |      0.8915 ns |       2.615 ns |  0.0063 |      40 B |
 |                 INPC012DontUseExpression | 1,720,614.95 ns | 39,412.5422 ns | 111,806.822 ns |       - |     480 B |
 |                         INPC013UseNameof | 1,781,528.42 ns | 38,773.1713 ns | 113,715.000 ns |  3.9063 |   36688 B |
