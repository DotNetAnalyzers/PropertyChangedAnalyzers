// ReSharper disable All
namespace ValidCode.Ignores;

using System.Collections;
using System.Collections.Generic;

public class IntEnumerator : IEnumerator<int>
{
    object IEnumerator.Current => Current;

    public int Current { get; private set; }

    public bool MoveNext()
    {
        switch (this.Current)
        {
            case int i
                when i < 5:
                this.Current = i + 1;
                return true;
            case -1:
                this.Current = 0;
                return true;
            default:
                return false;
        }
    }

    public void Reset()
    {
        this.Current = -1;
    }

    public void Dispose()
    {
    }
}
