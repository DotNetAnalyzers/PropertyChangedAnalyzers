``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 7 SP1 (6.1.7601.0)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410107 Hz, Resolution=293.2459 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0


```
|                        Method |     Mean |     Error |    StdDev | Allocated |
|------------------------------ |---------:|----------:|----------:|----------:|
| RunOnPropertyChangedAnalyzers | 2.026 ms | 0.0401 ms | 0.1005 ms |     880 B |
