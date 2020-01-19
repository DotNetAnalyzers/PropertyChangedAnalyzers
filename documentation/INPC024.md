# INPC024
## ReferenceEquals is always false for value types.

| Topic    | Value
| :--      | :--
| Id       | INPC024
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [SetAccessorAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/SetAccessorAnalyzer.cs)


## Description

ReferenceEquals is always false for value types.

## Motivation

Using `ReferenceEquals` in the sample below is a bug as it is always false.

```cs
public int P
{
    get => this.p;
    set
    {
        if (ReferenceEquals(value, this.p))
        {
            return;
        }

        this.p = value;
        this.OnPropertyChanged();
    }
}
```

## How to fix violations

Use the code fix to change it to:

```cs
public int P
{
    get => this.p;
    set
    {
        if (value == this.p)
        {
            return;
        }

        this.p = value;
        this.OnPropertyChanged();
    }
}
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC024 // ReferenceEquals is always false for value types.
Code violating the rule here
#pragma warning restore INPC024 // ReferenceEquals is always false for value types.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC024 // ReferenceEquals is always false for value types.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC024:ReferenceEquals is always false for value types.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->