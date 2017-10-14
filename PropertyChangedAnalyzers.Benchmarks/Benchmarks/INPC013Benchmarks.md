``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410117 Hz, Resolution=293.2451 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2114.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2114.0


```
 |                                 Method |     Mean |    Error |   StdDev |    Gen 0 |   Gen 1 | Allocated |
 |--------------------------------------- |---------:|---------:|---------:|---------:|--------:|----------:|
 | RunOnPropertyChangedAnalyzersAnalyzers | 60.88 ms | 1.313 ms | 3.871 ms | 937.5000 | 62.5000 |   6.01 MB |
