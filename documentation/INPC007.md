# INPC007
## The class has PropertyChangedEvent but no invoker.

| Topic    | Value
| :--      | :--
| Id       | INPC007
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [INPC007MissingInvoker]([INPC007MissingInvoker](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/INPC007MissingInvoker.cs))

## Description

The class has PropertyChangedEvent but no invoker.

## Motivation

ADD MOTIVATION HERE

## How to fix violations

ADD HOW TO FIX VIOLATIONS HERE

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC007 // The class has PropertyChangedEvent but no invoker.
Code violating the rule here
#pragma warning restore INPC007 // The class has PropertyChangedEvent but no invoker.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC007 // The class has PropertyChangedEvent but no invoker.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC007:The class has PropertyChangedEvent but no invoker.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->