``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 2 (10.0.15063)
Processor=Intel Core i7-3667U CPU 2.00GHz (Ivy Bridge), ProcessorCount=4
Frequency=2435876 Hz, Resolution=410.5299 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2115.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2115.0


```
 |                                 Method |     Mean |     Error |    StdDev |   Gen 0 | Allocated |
 |--------------------------------------- |---------:|----------:|----------:|--------:|----------:|
 | RunOnPropertyChangedAnalyzersAnalyzers | 17.10 ms | 0.2541 ms | 0.2253 ms | 93.7500 | 315.92 KB |
