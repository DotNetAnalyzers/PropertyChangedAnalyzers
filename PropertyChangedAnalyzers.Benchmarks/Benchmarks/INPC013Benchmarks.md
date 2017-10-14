``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410107 Hz, Resolution=293.2459 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2114.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2114.0


```
 |                                 Method |     Mean |    Error |   StdDev |   Median |    Gen 0 |   Gen 1 | Allocated |
 |--------------------------------------- |---------:|---------:|---------:|---------:|---------:|--------:|----------:|
 | RunOnPropertyChangedAnalyzersAnalyzers | 39.34 ms | 2.178 ms | 6.423 ms | 44.06 ms | 218.7500 | 62.5000 |   1.52 MB |
