namespace PropertyChangedAnalyzers.Test
{
    public static class Code
    {
        /// <summary>
        /// A class that uses underscore field conventions.
        /// </summary>
        public const string UnqualifiedUnderscoreFields = @"
namespace N
{
    using System;
    class UnqualifiedUnderscoreFields
    {
        private readonly int _f;
        UnqualifiedUnderscoreFields()
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
