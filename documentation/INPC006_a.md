# INPC006_a
## Check if value is different using ReferenceEquals before notifying.

<!-- start generated table -->
<table>
  <tr>
    <td>CheckId</td>
    <td>INPC006_a</td>
  </tr>
  <tr>
    <td>Severity</td>
    <td>Hidden</td>
  </tr>
  <tr>
    <td>Enabled</td>
    <td>true</td>
  </tr>
  <tr>
    <td>Category</td>
    <td>PropertyChangedAnalyzers.PropertyChanged</td>
  </tr>
  <tr>
    <td>Code</td>
    <td><a href="https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/NodeAnalyzers/IfStatementAnalyzer.cs">IfStatementAnalyzer</a></td>
  </tr>
</table>
<!-- end generated table -->

## Description

Check if value is different using ReferenceEquals before notifying.

## Motivation

This is a convenience analyzer if you want to make sure to use ReferenceEquals before notifying for reference types.
Note that INPC006_a must be disabled for when this rule is used.
This rule is enabled by default.

```c#
public Foo Bar
{
    get { return this.bar; }
    set
    {
        if (Equals(value, this.bar))
        {
            return;
        }

        this.bar = value;
        this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
    }
}
```

## How to fix violations

Use the code fix or manually change to using ReferenceEquals

```c#
public Foo Bar
{
    get { return this.bar; }
    set
    {
        if (ReferenceEquals(value, this.bar))
        {
            return;
        }

        this.bar = value;
        this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
    }
}
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC006_a // Check if value is different using ReferenceEquals before notifying.
Code violating the rule here
#pragma warning restore INPC006_a // Check if value is different using ReferenceEquals before notifying.
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC006_a // Check if value is different using ReferenceEquals before notifying.
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC006_a:Check if value is different using ReferenceEquals before notifying.", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->