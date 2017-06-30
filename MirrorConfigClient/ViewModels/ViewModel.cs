using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace MirrorConfigClient.ViewModels
{
    public class ViewModel<T> : DynamicObject, INotifyPropertyChanged, INotifyPropertyChangedExtended
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedExtendedEventHandler PropertyChangedExtended;

        #region [ctor]
        public ViewModel(T BusinessObject)
        {
            this.BusinessObject = BusinessObject;
            Properties = new Dictionary<string, KeyValuePair<PropertyInfo, bool>>();
            PropertyTargets = new Dictionary<string, object>();

            if (BusinessObject == null)
                return;

            foreach (var item in BusinessObject.GetType().GetProperties())
                AddProperty(item.Name, item, BusinessObject);
        }
        #endregion

        #region [Properties]

        public T BusinessObject { get; private set; }

        private Dictionary<string, KeyValuePair<PropertyInfo, bool>> Properties { get; set; }

        private Dictionary<string, object> PropertyTargets { get; set; }

        #endregion

        #region [DynamicPoperties]

        public void AddProperty(string Name, PropertyInfo Info, object TargetObject, bool TrackChanges = true)
        {
            Properties[Name] = new KeyValuePair<PropertyInfo, bool>(Info, TrackChanges);
            PropertyTargets[Name] = TargetObject;
        }

        protected void SetTracking(string Name, bool TrackChanges)
        {
            if (Properties.ContainsKey(Name))
                Properties[Name] = new KeyValuePair<PropertyInfo, bool>(Properties[Name].Key, TrackChanges);
        }

        public object this[string name]
        {
            get
            {
                return Properties[name].Key.GetValue(PropertyTargets[name]);
            }
            set
            {
                object OldValue = Properties[name].Key.GetValue(PropertyTargets[name]);
                Properties[name].Key.SetValue(PropertyTargets[name], value);
                OnPropertyChanged(name, OldValue, value);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (Properties.ContainsKey(binder.Name))
            {
                result = this[binder.Name];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (Properties.ContainsKey(binder.Name))
            {
                this[binder.Name] = value;
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Gets the specified property of the BusinessObject.
        /// </summary>
        /// <example>
        /// string test = vm.Get(() => vm.BusinessObject.Title); 
        /// For more Performance use: 
        /// string test = vm[nameof(vm.BusinessObject.Title)]; 
        /// Or call OnPropertyChanged after modifing the BusinessObject.
        /// </example>
        /// <typeparam name="V">Type of Property</typeparam>
        /// <param name="property">The property from the BusinessObject.</param>
        /// <returns>The Value of the BusinessObject Property</returns>
        public V Get<V>(Expression<Func<V>> property)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            return (V)this[propertyInfo.Name];
        }

        /// <summary>
        /// Sets the specified propertyof the BusinessObject.
        /// </summary>
        /// <example>
        /// vm.Set(() => vm.BusinessObject.Title, "Test"); 
        /// or more Performance use: 
        /// vm[nameof(vm.BusinessObject.Title)] = "Test"; 
        /// Or call OnPropertyChanged after modifing the BusinessObject.
        /// </example>
        /// <typeparam name="V">Type of Property</typeparam>
        /// <param name="property">The property from the BusinessObject.</param>
        /// <param name="value">The value.</param>
        public void Set<V>(Expression<Func<V>> property, V value)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            this[propertyInfo.Name] = value;
        }

        #endregion

        #region [PropertyChanged]
        public void OnPropertyChanged(string Name, object OldValue, object NewValue)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
            if (Properties.ContainsKey(Name) && Properties[Name].Value && (OldValue != null || NewValue != null))
                PropertyChangedExtended?.Invoke(this, new PropertyChangedExtendedEventArgs(Name, OldValue, NewValue));
        }
        #endregion
    }
}
