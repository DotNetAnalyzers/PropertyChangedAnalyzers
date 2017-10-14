``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410107 Hz, Resolution=293.2459 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2114.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2114.0


```
 |                                 Method |     Mean |    Error |   StdDev |    Gen 0 |    Gen 1 | Allocated |
 |--------------------------------------- |---------:|---------:|---------:|---------:|---------:|----------:|
 | RunOnPropertyChangedAnalyzersAnalyzers | 55.02 ms | 1.069 ms | 1.756 ms | 937.5000 | 125.0000 |   6.05 MB |
