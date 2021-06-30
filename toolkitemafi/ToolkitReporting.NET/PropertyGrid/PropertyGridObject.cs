using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Windows.Forms;
using Eff.Utils;

// ReSharper disable once CheckNamespace
namespace Eff.ToolkitReporting.NET.PropertyGrid
{
    public class PropertyGridObjectProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public Attribute Attribute { get; set; }
        public string Category { get; set; }
        public bool ReadOnly { get; set; }
    }

    public class PropertyGridObject : DynamicObject, ICustomTypeDescriptor, INotifyPropertyChanged
    {
        private readonly IDictionary<string, PropertyGridObjectProperty> _dynamicProperties = new Dictionary<string, PropertyGridObjectProperty>();

        public void AddNewProperty(string name, object value, string display = null,
            string desc = null, Attribute attribute = null, string category = null, bool readOnly = false)
        {
            if (display == null) display = name;
            if (attribute == null) attribute = new EditorAttribute();
            var propertyObj = new PropertyGridObjectProperty
            {
                Name = name,
                Value = value,
                DisplayName = display,
                Description = desc,
                Attribute = attribute,
                Category = category,
                ReadOnly =  readOnly
            };
            _dynamicProperties.Add(name, propertyObj);
        }

        //protected virtual void SetMemberValue(string memberName, string value)
        //{
        //}

        public object GetMember(string memberName)
        {
            PropertyGridObjectProperty result;
            _dynamicProperties.TryGetValue(memberName, out result);
            return result == null ? null : result.Value;
        }

        public void SetMember(string memberName, object value)
        {
            try
            {
                EmcLog.Debug("BEGIN");
                _dynamicProperties[memberName].Value = value;
                //var paramValue = (string)value;
                //SetMemberValue(memberName, paramValue);
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                MessageBox.Show(string.Format("Error: {0}", e.Message), MainClass.Caption, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                EmcLog.Debug("END");
            }

            NotifyToRefreshAllProperties();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetMember(binder.Name);
            return result != null;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            SetMember(binder.Name, value);
            return true;
        }
        #region Implementation of ICustomTypeDescriptor

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            // ReSharper disable once CoVariantArrayConversion
            return new PropertyDescriptorCollection(_dynamicProperties
                .Select(x =>
                {
                    var newAttributes = new[]
                    {
                        new CategoryAttribute(x.Value.Category),
                        new DisplayNameAttribute(x.Value.DisplayName), 
                        new DescriptionAttribute(x.Value.Description),
                        new ReadOnlyAttribute(x.Value.ReadOnly),
                        x.Value.Attribute
                    };
                    var allAttributes = new Attribute[attributes.Length + newAttributes.Length];
                    Array.Copy(attributes, allAttributes, attributes.Length);
                    Array.Copy(newAttributes, 0, allAttributes, attributes.Length, newAttributes.Length);
                    return new DynamicPropertyDescriptor(this,
                        x.Key, x.Value.Value.GetType(), allAttributes);
                }).ToArray());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        public string GetClassName()
        {
            return GetType().Name;
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            var eventArgs = new PropertyChangedEventArgs(propertyName);
            PropertyChanged(this, eventArgs);
        }

        private void NotifyToRefreshAllProperties()
        {
            OnPropertyChanged(String.Empty);
        }

        #endregion

        #region Hide not implemented members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetComponentName()
        {
            throw new NotImplementedException();
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        #endregion

        private class DynamicPropertyDescriptor : PropertyDescriptor
        {
            private readonly PropertyGridObject _propertyGridObject;
            private readonly Type _propertyType;
            private readonly Attribute[] _propertyAttributes;

            public DynamicPropertyDescriptor(PropertyGridObject propertyGridObject,
                string propertyName, Type propertyType, Attribute[] propertyAttributes)
                : base(propertyName, propertyAttributes)
            {
                _propertyGridObject = propertyGridObject;
                _propertyType = propertyType;
                _propertyAttributes = propertyAttributes;
            }

            public override bool CanResetValue(object component)
            {
                return true;
            }

            public override object GetValue(object component)
            {
                return _propertyGridObject.GetMember(Name);
            }

            public override void ResetValue(object component)
            {
            }

            public override void SetValue(object component, object value)
            {
                _propertyGridObject.SetMember(Name, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return typeof(ReportingObject); }
            }

            public override bool IsReadOnly
            {
                get
                {
                    var result = _propertyAttributes.Any(x => x is ReadOnlyAttribute && x.Equals(ReadOnlyAttribute.Yes));
                    return result;
                }
            }

            public override Type PropertyType
            {
                get { return _propertyType; }
            }
        }
    }
}
