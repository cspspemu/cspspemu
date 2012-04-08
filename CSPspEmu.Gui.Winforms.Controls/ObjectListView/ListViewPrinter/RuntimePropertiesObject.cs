using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace BrightIdeasSoftware
{

    /// <summary>
    /// A CustomPropertyDescriptor allows properties to be added to a property grid  
    /// whilst allowing values to be fetched and set on the owning object.
    /// </summary>
    /// <remarks>In the GetValue(), SetValue() and ResetValue() methods the
    /// 'component' object is the owner of the property.</remarks>
    internal class CustomPropertyDescriptor : PropertyDescriptor
    {
        public CustomPropertyDescriptor(string name, Type type, string category)
            : base(name, new Attribute[] { })
        {
            this.name = name;
            this.propertyType = type;
            this.category = category;
        }
        string name;
        Type propertyType;
        string category;

        public override string Category
        {
            get { return category; }
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get { return typeof(RuntimePropertyObject); }
        }

        public override object GetValue(object component)
        {
            return ((RuntimePropertyObject)component).GetValue(this.name);
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return this.propertyType; }
        }

        public override void ResetValue(object component)
        {
            ((RuntimePropertyObject)component).ResetValue(this.name);
        }

        public override void SetValue(object component, object value)
        {
            ((RuntimePropertyObject)component).SetValue(this.name, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }

    /// <summary>
    /// A RuntimePropertyObject can construct its visible properties at runtime,
    /// not through reflection.
    /// </summary>
    /// <remarks>
    /// Instances of this class can have their properties constructed at
    /// runtime in such a way that a PropertyGrid can read and modify its 
    /// properties. A PropertyGrid normally uses reflection to decide
    /// the properties to be presents, which means the characteristics of
    /// an object are decided at runtime. In contrast, this object 
    /// properties are determined by the AddProperty() calls that are made
    /// on it.
    /// </remarks>
    internal class RuntimePropertyObject : CustomTypeDescriptor
    {
        #region Property Management

        public void AddProperty(string name, Type propertyType)
        {
            this.propertyDescriptions.Add(new CustomPropertyDescriptor(name, propertyType, this.propertyCategory));
        }

        public void AddProperty(string name, Type propertyType, object defaultValue)
        {
            this.AddProperty(name, propertyType);
            this.valueMap[name] = defaultValue;
        }

        public void SetPropertyCategory(string category)
        {
            this.propertyCategory = category;
        }

        string propertyCategory;
        List<CustomPropertyDescriptor> propertyDescriptions = new List<CustomPropertyDescriptor>();

        #endregion

        #region Property values

        public object GetValue(string name)
        {
            object value;

            if (this.valueMap.TryGetValue(name, out value))
                return value;
            else
                return null;
        }

        public void SetValue(string name, object value)
        {
            this.valueMap[name] = value;
        }

        public void ResetValue(string name)
        {
            this.valueMap[name] = null;
        }

        Dictionary<string, object> valueMap = new Dictionary<string, object>();

        #endregion

        #region Overrides of CustomTypeDescriptor

        // This is the part that determines what properties are shown by the PropertyGrid

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            // The value returned by this method is the 'component' parameter of the 
            // CustomPropertyDescriptor.GetValue, SetValue and ResetValue methods.
            return this;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(propertyDescriptions.ToArray());
        }

        #endregion
    }
}
