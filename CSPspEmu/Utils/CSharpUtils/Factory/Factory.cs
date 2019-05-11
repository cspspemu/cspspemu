namespace CSharpUtils.Factory
{
    /// <summary>
    /// 
    /// </summary>
    public class Factory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T New<T>() where T : new()
        {
            return new T();
        }
    }
}