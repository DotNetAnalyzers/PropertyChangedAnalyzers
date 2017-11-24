``` ini

BenchmarkDotNet=v0.10.10, OS=Windows 7 SP1 (6.1.7601.0)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410126 Hz, Resolution=293.2443 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0


```
|                        Method |     Mean |     Error |   StdDev |   Median |  Gen 0 | Allocated |
|------------------------------ |---------:|----------:|---------:|---------:|-------:|----------:|
| RunOnPropertyChangedAnalyzers | 34.60 us | 0.6909 us | 1.154 us | 33.97 us | 0.1221 |     960 B |
