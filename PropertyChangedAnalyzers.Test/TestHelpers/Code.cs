namespace PropertyChangedAnalyzers.Test
{
    public static class Code
    {
        /// <summary>
        /// A class that uses underscore field conventions.
        /// </summary>
        public const string UnderScoreFieldsUnqualified = @"
namespace N
{
    using System;
    class UnderScoreFieldsUnqualified
    {
        private readonly int _f;
        UnderScoreFieldsUnqualified()
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
