# INPC002
## Mutable public property should notify

| Topic    | Value
| :--      | :--
| Id       | INPC002
| Severity | Warning
| Enabled  | True
| Category | PropertyChangedAnalyzers.PropertyChanged
| Code     | [SetAccessorAnalyzer](https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers/blob/master/PropertyChangedAnalyzers/Analyzers/SetAccessorAnalyzer.cs)

## Description

All mutable public properties should notify when their value changes.

## Motivation

Properties not notifying when their value changes is a common source of bugs in WPF.
It results in bindings not working.
*actually bindings can work any way but it is fragile to rely on it. Also always nice to be explicit about what the code is meant to do.

In the following example the Value property is updated but does not notify. This would mean that if there is a view binding to Value it will not update as it should.

```C#
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class MyViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public int Value { get; private set; }

    public void Update()
    {
        this.Value++;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

## How to fix violations

Fix the warning by either

a) Make the property get-only:

```C#
public class MyViewModel
{
    public int Value { get; } = 5;
}
```

b) Raise property changed when the property changes:

```C#
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class MyViewModel : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value
    {
        get
        {
            return this.value;
        }
        private set
        {
            if (value == this.value)
            {
                return;
            }

            this.value = value;
            this.OnPropertyChanged();
        }
    }

    public void Update()
    {
        this.Value++;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

<!-- start generated config severity -->
## Configure severity

### Via ruleset file.

Configure the severity per project, for more info see [MSDN](https://msdn.microsoft.com/en-us/library/dd264949.aspx).

### Via #pragma directive.
```C#
#pragma warning disable INPC002 // Mutable public property should notify
Code violating the rule here
#pragma warning restore INPC002 // Mutable public property should notify
```

Or put this at the top of the file to disable all instances.
```C#
#pragma warning disable INPC002 // Mutable public property should notify
```

### Via attribute `[SuppressMessage]`.

```C#
[System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", 
    "INPC002:Mutable public property should notify", 
    Justification = "Reason...")]
```
<!-- end generated config severity -->