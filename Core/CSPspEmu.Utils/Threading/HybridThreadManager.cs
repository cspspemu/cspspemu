namespace CSPspEmu.Utils.Threading
{
    public class HybridThreadManager
    {
        protected bool Parallel;

        public HybridThreadManager(bool Parallel = true)
        {
            this.Parallel = Parallel;
        }
    }
}