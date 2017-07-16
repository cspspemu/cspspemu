using System;
using System.Linq;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class FieldMapping : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string ThisField;

        /// <summary>
        /// 
        /// </summary>
        public string ConfigurationField;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static object ObjectFieldGet(object Object, string fieldName)
        {
            //Console.WriteLine("(" + Object.GetType() + ").(" + FieldName + ")");
            try
            {
                var field = Object.GetType().GetField(fieldName);
                if (field != null) return field.GetValue(Object);
                var property = Object.GetType().GetProperty(fieldName);
                if (property != null) return property.GetValue(Object, null);
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Can't get value: " + e);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        public static object ConvertTo(object Object, Type toType)
        {
            if (toType.IsEnum)
            {
                Object = Enum.ToObject(toType, Object);
            }
            return Convert.ChangeType(Object, toType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <exception cref="NotImplementedException"></exception>
        public static void ObjectFieldSet(object Object, string fieldName, object value)
        {
            //Console.WriteLine("(" + Object + ").(" + FieldName + ")=" + Value);
            try
            {
                var field = Object.GetType().GetField(fieldName);
                //Enum.ToObject(

                if (field != null)
                {
                    field.SetValue(Object, ConvertTo(value, field.FieldType));
                    return;
                }

                var property = Object.GetType().GetProperty(fieldName) ?? throw new NotImplementedException();
                property.SetValue(Object, ConvertTo(value, property.PropertyType), null);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Can't set value: " + e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="configuration"></param>
        public static void ObjectToConfiguration(object source, object configuration)
        {
            foreach (var sourceMember in source.GetType().GetFields())
            {
                foreach (var fieldMappingAttribute in sourceMember.GetCustomAttributes(typeof(FieldMapping), true)
                    .Cast<FieldMapping>())
                {
                    ObjectFieldSet(
                        configuration,
                        fieldMappingAttribute.ConfigurationField,
                        ObjectFieldGet(
                            ObjectFieldGet(source, sourceMember.Name),
                            fieldMappingAttribute.ThisField
                        )
                    );
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="destination"></param>
        public static void ConfigurationToObject(object configuration, object destination)
        {
            foreach (var destinationMember in destination.GetType().GetFields())
            {
                foreach (var fieldMappingAttribute in destinationMember.GetCustomAttributes(typeof(FieldMapping), true)
                    .Cast<FieldMapping>())
                {
                    ObjectFieldSet(
                        ObjectFieldGet(destination, destinationMember.Name),
                        fieldMappingAttribute.ThisField,
                        ObjectFieldGet(
                            configuration,
                            fieldMappingAttribute.ConfigurationField
                        )
                    );
                }
            }
        }
    }
}