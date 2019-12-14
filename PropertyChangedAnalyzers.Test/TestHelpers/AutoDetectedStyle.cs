namespace PropertyChangedAnalyzers.Test
{
    using System;
    using System.Collections.Generic;

    public readonly struct AutoDetectedStyle
    {
        private readonly string caption;
        private readonly Func<string, string> applyFieldNamingStyle;
        private readonly Func<string, string> applyFieldQualificationPreference;

        public AutoDetectedStyle(
            string caption,
            string additionalSample,
            Func<string, string> applyFieldNamingStyle,
            Func<string, string> applyFieldQualificationPreference)
        {
            this.caption = caption ?? throw new ArgumentNullException(nameof(caption));
            this.AdditionalSample = additionalSample ?? throw new ArgumentNullException(nameof(additionalSample));
            this.applyFieldNamingStyle = applyFieldNamingStyle ?? throw new ArgumentNullException(nameof(applyFieldNamingStyle));
            this.applyFieldQualificationPreference = applyFieldQualificationPreference ?? throw new ArgumentNullException(nameof(applyFieldQualificationPreference));
        }

        public string AdditionalSample { get; }

        public override string ToString() => this.caption;

        public string Apply(string source, params string[] namesToStyle)
        {
            foreach (var name in namesToStyle)
            {
                var styledName = this.applyFieldNamingStyle(name);

                source = source
                    .AssertReplaceWholeWord(name, styledName)
                    .ReplaceWholeWord("this." + styledName, this.applyFieldQualificationPreference(styledName));
            }

            return source;
        }
    }
}
