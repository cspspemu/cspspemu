namespace CSPspEmu.Utils.Threading
{
    public class HybridThreadManager
    {
        protected bool Parallel;

        public HybridThreadManager(bool parallel = true)
        {
            this.Parallel = parallel;
        }
    }
}