namespace PropertyChangedAnalyzers.Test
{
    public static class Code
    {
        /// <summary>
        /// A class that uses underscore field conventions.
        /// </summary>
        public const string UnderscoreFieldsUnqualified = @"
namespace N
{
    using System;
    class UnderscoreFieldsUnqualified
    {
        private readonly int _f;
        UnderscoreFieldsUnqualified()
        {
            _f = 1;
            P = 2;
            E?.Invoke();
            M();
        }

        public event Action E;

        public int P { get; }

        private int M() => _f;
    }
}";
    }
}
