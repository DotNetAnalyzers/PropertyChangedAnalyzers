``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410097 Hz, Resolution=293.2468 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                        Method |     Mean |    Error |   StdDev |  Gen 0 | Allocated |
 |------------------------------ |---------:|---------:|---------:|-------:|----------:|
 | RunOnPropertyChangedAnalyzers | 212.9 ns | 4.130 ns | 4.419 ns | 0.0699 |     440 B |
