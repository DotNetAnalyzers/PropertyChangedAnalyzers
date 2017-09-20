namespace PropertyChangedAnalyzers
{
    using System.Text;

    internal static class StringBuilderPool
    {
        private static readonly Pool<StringBuilder> Pool = new Pool<StringBuilder>(
            () => new StringBuilder(),
            x => x.Clear());

        public static Pool<StringBuilder>.Pooled Borrow()
        {
            return Pool.GetOrCreate();
        }
    }
}