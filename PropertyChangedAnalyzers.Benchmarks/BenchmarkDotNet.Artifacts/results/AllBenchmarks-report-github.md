``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410126 Hz, Resolution=293.2443 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2114.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2114.0


```
 |                                   Method |            Mean |          Error |         StdDev |          Median |   Gen 0 |   Gen 1 | Allocated |
 |----------------------------------------- |----------------:|---------------:|---------------:|----------------:|--------:|--------:|----------:|
 |   INPC001ImplementINotifyPropertyChanged |   165,340.55 ns |  3,291.1076 ns |   7,495.517 ns |   162,647.07 ns |  1.7090 |  1.7090 |    9740 B |
 | INPC002MutablePublicPropertyShouldNotify |    30,485.62 ns |    564.4358 ns |     500.358 ns |    30,580.12 ns |       - |       - |     244 B |
 |         INPC003NotifyWhenPropertyChanges | 2,557,600.43 ns | 50,930.0264 ns | 126,833.439 ns | 2,529,189.16 ns |       - |       - |   15424 B |
 |               INPC004UseCallerMemberName | 1,088,581.07 ns |  4,676.3268 ns |   3,650.968 ns | 1,088,606.89 ns |       - |       - |     272 B |
 |   INPC005CheckIfDifferentBeforeNotifying | 1,125,856.49 ns | 23,232.3559 ns |  58,711.128 ns | 1,122,480.52 ns |       - |       - |      32 B |
 |  INPC006UseObjectEqualsForReferenceTypes |   650,976.46 ns | 12,999.3620 ns |  38,124.878 ns |   646,686.65 ns |       - |       - |      32 B |
 |                INPC006UseReferenceEquals |   645,707.50 ns | 13,638.3370 ns |  40,212.944 ns |   646,323.03 ns |       - |       - |      32 B |
 |                    INPC007MissingInvoker |        43.33 ns |      3.3608 ns |       9.909 ns |        39.92 ns |  0.0045 |  0.0045 |      24 B |
 |               INPC008StructMustNotNotify |        33.93 ns |      1.0720 ns |       3.144 ns |        33.82 ns |  0.0045 |  0.0045 |      24 B |
 | INPC009DontRaiseChangeForMissingProperty | 2,124,621.85 ns | 64,108.6478 ns | 184,968.107 ns | 2,091,029.03 ns | 15.6250 | 15.6250 |   97377 B |
 |             INPC010SetAndReturnSameField |    41,730.39 ns |    952.6889 ns |   2,763.923 ns |    41,308.05 ns |  0.2441 |  0.2441 |    1572 B |
 |                        INPC011DontShadow |        34.16 ns |      0.7168 ns |       1.926 ns |        33.65 ns |  0.0045 |  0.0045 |      24 B |
 |                 INPC012DontUseExpression | 1,578,251.39 ns | 31,232.3198 ns |  88,600.893 ns | 1,570,496.32 ns |       - |       - |     256 B |
 |                         INPC013UseNameof | 1,667,146.78 ns | 34,933.9547 ns |  80,964.752 ns | 1,651,439.16 ns |  1.9531 |  1.9531 |   18640 B |
