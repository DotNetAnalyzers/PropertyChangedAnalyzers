namespace PropertyChangedAnalyzers.Test
{
    using System.Collections.Generic;

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

        /// <summary>
        /// A class that uses both underscore field conventions and qualified field access.
        /// </summary>
        public const string QualifiedUnderscoreFields = @"
namespace N
{
    using System;
    class QualifiedUnderscoreFields
    {
        private readonly int _f;
        QualifiedUnderscoreFields()
        {
            this._f = 1;
            P = 2;
            E?.Invoke();
            M();
        }

        public event Action E;

        public int P { get; }

        private int M() => this._f;
    }
}";

        /// <summary>
        /// A class that does not use underscore field conventions but which also does not qualify field access.
        /// </summary>
        public const string UnqualifiedUnprefixedFields = @"
namespace N
{
    using System;
    class UnqualifiedUnprefixedFields
    {
        private readonly int f;
        UnqualifiedUnprefixedFields()
        {
            f = 1;
            P = 2;
            E?.Invoke();
            M();
        }

        public event Action E;

        public int P { get; }

        private int M() => f;
    }
}";

        /// <summary>
        /// A class that does not use underscore field conventions and which qualifies field access.
        /// </summary>
        public const string QualifiedUnprefixedFields = @"
namespace N
{
    using System;
    class QualifiedUnprefixedFields
    {
        private readonly int f;
        QualifiedUnprefixedFields()
        {
            this.f = 1;
            P = 2;
            E?.Invoke();
            M();
        }

        public event Action E;

        public int P { get; }

        private int M() => this.f;
    }
}";

        public static IEnumerable<AutoDetectedStyle> AutoDetectedStyles { get; } = new[]
        {
            new AutoDetectedStyle(
                nameof(Code.UnqualifiedUnderscoreFields),
                Code.UnqualifiedUnderscoreFields,
                applyFieldNamingStyle: name => name.EnsurePrefix("_"),
                applyFieldQualificationPreference: syntax => syntax.RemovePrefix("this.")),

            new AutoDetectedStyle(
                nameof(Code.QualifiedUnderscoreFields),
                Code.QualifiedUnderscoreFields,
                applyFieldNamingStyle: name => name.EnsurePrefix("_"),
                applyFieldQualificationPreference: syntax => syntax.EnsurePrefix("this.")),

            new AutoDetectedStyle(
                nameof(Code.UnqualifiedUnprefixedFields),
                Code.UnqualifiedUnprefixedFields,
                applyFieldNamingStyle: name => name.RemovePrefix("_"),
                applyFieldQualificationPreference: syntax => syntax.RemovePrefix("this.")),

            new AutoDetectedStyle(
                nameof(Code.QualifiedUnprefixedFields),
                Code.QualifiedUnprefixedFields,
                applyFieldNamingStyle: name => name.RemovePrefix("_"),
                applyFieldQualificationPreference: syntax => syntax.EnsurePrefix("this.")),
        };
    }
}
