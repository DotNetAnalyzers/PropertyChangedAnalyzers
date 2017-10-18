``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410126 Hz, Resolution=293.2443 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2114.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2114.0


```
 |                                   Method |            Mean |          Error |         StdDev |          Median |   Gen 0 |  Gen 1 | Allocated |
 |----------------------------------------- |----------------:|---------------:|---------------:|----------------:|--------:|-------:|----------:|
 |   INPC001ImplementINotifyPropertyChanged |   182,658.38 ns |  6,629.3421 ns |  17,695.056 ns |   179,756.64 ns |  1.7090 | 1.7090 |    9672 B |
 | INPC002MutablePublicPropertyShouldNotify |    32,892.01 ns |  1,022.7271 ns |   2,999.482 ns |    32,811.71 ns |       - |      - |     244 B |
 |         INPC003NotifyWhenPropertyChanges | 2,717,700.37 ns | 62,361.9652 ns | 180,923.369 ns | 2,711,729.09 ns |       - |      - |   15419 B |
 |               INPC004UseCallerMemberName | 1,238,335.68 ns | 40,183.4150 ns | 117,851.000 ns | 1,236,045.79 ns |       - |      - |     272 B |
 |   INPC005CheckIfDifferentBeforeNotifying | 1,319,629.83 ns | 84,690.7814 ns | 249,712.680 ns | 1,197,863.49 ns |       - |      - |      32 B |
 |  INPC006UseObjectEqualsForReferenceTypes |   620,422.22 ns | 12,653.6769 ns |  36,508.751 ns |   616,494.73 ns |       - |      - |      32 B |
 |                INPC006UseReferenceEquals |   614,839.69 ns | 16,097.0330 ns |  47,462.465 ns |   596,837.20 ns |       - |      - |      32 B |
 |                    INPC007MissingInvoker |        34.65 ns |      0.9380 ns |       2.751 ns |        34.30 ns |  0.0045 | 0.0045 |      24 B |
 |               INPC008StructMustNotNotify |        32.04 ns |      0.6929 ns |       2.010 ns |        31.87 ns |  0.0045 | 0.0045 |      24 B |
 | INPC009DontRaiseChangeForMissingProperty | 2,172,400.56 ns | 13,924.3960 ns |  11,627.500 ns | 2,167,462.81 ns | 35.1563 |      - |  199809 B |
 |             INPC010SetAndReturnSameField |    40,002.61 ns |    796.7847 ns |   1,954.526 ns |    39,911.80 ns |  0.2441 | 0.2441 |    1572 B |
 |                        INPC011DontShadow |        31.94 ns |      0.6695 ns |       1.604 ns |        31.50 ns |  0.0045 | 0.0045 |      24 B |
 |                 INPC012DontUseExpression | 1,535,292.09 ns | 30,516.0019 ns |  63,698.260 ns | 1,527,046.68 ns |       - |      - |     256 B |
 |                         INPC013UseNameof | 1,570,255.97 ns | 30,648.5879 ns |  56,042.676 ns | 1,559,187.71 ns |  1.9531 | 1.9531 |   18336 B |
