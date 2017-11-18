``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410107 Hz, Resolution=293.2459 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                        Method |     Mean |     Error |    StdDev |   Gen 0 | Allocated |
 |------------------------------ |---------:|----------:|----------:|--------:|----------:|
 | RunOnPropertyChangedAnalyzers | 2.866 ms | 0.0695 ms | 0.2049 ms | 27.3438 |    198 KB |
