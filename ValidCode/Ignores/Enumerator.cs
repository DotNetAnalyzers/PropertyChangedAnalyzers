namespace ValidCode.Ignores
{
    using System.Collections;

    public class Enumerator : IEnumerator
    {
        public object Current { get; private set; }

        public bool MoveNext()
        {
            switch (Current)
            {
                case int i
                    when i < 5:
                    Current = i + 1;
                    return true;
                case null:
                    Current = 0;
                    return true;
                default:
                    return false;
            }
        }

        public void Reset()
        {
            Current = null;
        }
    }
}
