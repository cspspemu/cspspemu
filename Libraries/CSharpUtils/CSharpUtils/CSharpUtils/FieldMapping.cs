using System;
using System.Linq;

namespace CSharpUtils
{
    public class FieldMapping : Attribute
    {
        public String ThisField;
        public String ConfigurationField;

        public static Object ObjectFieldGet(Object Object, String FieldName)
        {
            //Console.WriteLine("(" + Object.GetType() + ").(" + FieldName + ")");
            try
            {
                var Field = Object.GetType().GetField(FieldName);
                if (Field != null) return Field.GetValue(Object);
                var Property = Object.GetType().GetProperty(FieldName);
                if (Property != null) return Property.GetValue(Object, null);
                throw(new NotImplementedException());
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Can't get value: " + e);
                return null;
            }
        }

        public static Object ConvertTo(Object Object, Type ToType)
        {
            if (ToType.IsEnum)
            {
                Object = Enum.ToObject(ToType, Object);
            }
            return Convert.ChangeType(Object, ToType);
        }

        public static void ObjectFieldSet(Object Object, String FieldName, Object Value)
        {
            //Console.WriteLine("(" + Object + ").(" + FieldName + ")=" + Value);
            try
            {
                var Field = Object.GetType().GetField(FieldName);
                //Enum.ToObject(

                if (Field != null)
                {
                    Field.SetValue(Object, ConvertTo(Value, Field.FieldType));
                    return;
                }
                var Property = Object.GetType().GetProperty(FieldName);
                if (Property != null)
                {
                    Property.SetValue(Object, ConvertTo(Value, Property.PropertyType), null);
                    return;
                }
                throw (new NotImplementedException());
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Can't set value: " + e);
            }
        }

        public static void ObjectToConfiguration(Object Source, Object Configuration)
        {
            foreach (var SourceMember in Source.GetType().GetFields())
            {
                foreach (var FieldMappingAttribute in SourceMember.GetCustomAttributes(typeof(FieldMapping), true)
                    .Cast<FieldMapping>())
                {
                    ObjectFieldSet(
                        Configuration,
                        FieldMappingAttribute.ConfigurationField,
                        ObjectFieldGet(
                            ObjectFieldGet(Source, SourceMember.Name),
                            FieldMappingAttribute.ThisField
                        )
                    );
                }
            }
        }

        public static void ConfigurationToObject(Object Configuration, Object Destination)
        {
            foreach (var DestinationMember in Destination.GetType().GetFields())
            {
                foreach (var FieldMappingAttribute in DestinationMember.GetCustomAttributes(typeof(FieldMapping), true)
                    .Cast<FieldMapping>())
                {
                    ObjectFieldSet(
                        ObjectFieldGet(Destination, DestinationMember.Name),
                        FieldMappingAttribute.ThisField,
                        ObjectFieldGet(
                            Configuration,
                            FieldMappingAttribute.ConfigurationField
                        )
                    );
                }
            }
        }
    }
}