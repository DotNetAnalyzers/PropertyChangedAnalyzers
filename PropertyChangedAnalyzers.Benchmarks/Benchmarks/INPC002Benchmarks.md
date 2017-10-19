``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410126 Hz, Resolution=293.2443 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2114.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2114.0


```
 |                        Method |     Mean |    Error |   StdDev |   Median | Allocated |
 |------------------------------ |---------:|---------:|---------:|---------:|----------:|
 | RunOnPropertyChangedAnalyzers | 36.19 us | 2.515 us | 7.417 us | 32.23 us |     244 B |
