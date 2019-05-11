using System;
using System.Collections.Generic;

namespace CSharpUtils.Factory
{
    /// <summary>
    /// 
    /// </summary>
    public class MockeableFactory : Factory
    {
        Dictionary<Type, Type> MockedTypes = new Dictionary<Type, Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeToMock"></param>
        /// <param name="mockedType"></param>
        public void MockType(Type typeToMock, Type mockedType)
        {
            MockedTypes[typeToMock] = mockedType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T New<T>()
        {
            if (MockedTypes.ContainsKey(typeof(T)))
            {
                return (T) Activator.CreateInstance(MockedTypes[typeof(T)]);
            }

            return base.New<T>();
        }
    }
}