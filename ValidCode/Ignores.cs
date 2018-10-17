// ReSharper disable All
namespace ValidCode
{
    public class Ignores
    {
        private int setOnly;

        public Ignores(int only)
        {
            this.GetOnly = only;
            this.setOnly = only;
            this.PublicGetPrivateSet = only;
        }

        public int GetOnly { get; }

        public int SetOnly
        {
            set => this.setOnly = value;
        }

        /// <summary>
        /// Assigned in ctor only is ok.
        /// </summary>
        public int PublicGetPrivateSet { get; private set; }

        public int ExpressionBody => 1;
    }
}
