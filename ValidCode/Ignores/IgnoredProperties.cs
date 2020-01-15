// ReSharper disable All
namespace ValidCode.Ignores
{
    public class IgnoredProperties
    {
        public IgnoredProperties(int privateSetAssignedInConstructor)
        {
            this.PrivateSetAssignedInConstructor = privateSetAssignedInConstructor;
        }

        public static int P { get; set; }

        public int GetOnly { get; } = 1;

        public int ExpressionBody => 1;

        public int ExpressionBodyGetter
        {
            get => 1;
        }

        public int StatementBodyGetter
        {
#pragma warning disable INPC020 // Prefer expression body accessor.
            get { return 1; }
#pragma warning restore INPC020
        }

        public int PrivateSetAssignedInConstructor { get; private set; }
    }
}
