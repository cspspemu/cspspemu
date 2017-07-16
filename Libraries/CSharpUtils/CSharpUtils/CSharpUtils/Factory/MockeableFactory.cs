using System;
using System.Collections.Generic;

namespace CSharpUtils.Factory
{
    public class MockeableFactory : Factory
    {
        Dictionary<Type, Type> MockedTypes = new Dictionary<Type, Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TypeToMock"></param>
        /// <param name="MockedType"></param>
        public void MockType(Type TypeToMock, Type MockedType)
        {
            MockedTypes[TypeToMock] = MockedType;
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