``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.523 (1803/April2018Update/Redstone4)
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical and 8 physical cores
Frequency=3410073 Hz, Resolution=293.2489 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3260.0
  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3260.0


```
|                                 Method |         Mean |       Error |      StdDev |       Median | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|--------------------------------------- |-------------:|------------:|------------:|-------------:|------------:|------------:|------------:|--------------------:|
|                       ArgumentAnalyzer | 2,829.804 us |  55.9008 us | 127.3144 us | 2,805.658 us |           - |           - |           - |             57344 B |
|                     AssignmentAnalyzer |   256.499 us |   5.1179 us |   9.9821 us |   256.006 us |           - |           - |           - |                   - |
|                    IfStatementAnalyzer |   391.796 us |   8.4200 us |  24.2937 us |   383.276 us |           - |           - |           - |                   - |
|                     InvocationAnalyzer | 3,927.694 us | 102.5622 us | 109.7404 us | 3,895.078 us |           - |           - |           - |            106496 B |
|              MethodDeclarationAnalyzer |   484.774 us |   9.8927 us |  28.3840 us |   473.597 us |           - |           - |           - |             16384 B |
|            PropertyDeclarationAnalyzer | 2,327.676 us |  58.8597 us | 164.0774 us | 2,327.956 us |           - |           - |           - |             65536 B |
| INPC001ImplementINotifyPropertyChanged |   409.644 us |   8.1417 us |  21.7318 us |   399.112 us |           - |           - |           - |             32768 B |
|       INPC003NotifyWhenPropertyChanges | 7,401.852 us |  86.8849 us |  77.0212 us | 7,397.642 us |           - |           - |           - |            212992 B |
|                  INPC007MissingInvoker |   405.306 us |   8.4791 us |  24.7338 us |   392.220 us |           - |           - |           - |             16384 B |
|             INPC008StructMustNotNotify |     2.536 us |   0.0611 us |   0.1714 us |     2.639 us |           - |           - |           - |                   - |
|                      INPC011DontShadow |   160.867 us |   4.1669 us |  11.4769 us |   158.941 us |           - |           - |           - |                   - |
