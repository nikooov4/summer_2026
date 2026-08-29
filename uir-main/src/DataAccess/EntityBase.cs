using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Mephi.Cybernetics.Race.DataAccess.DataSources;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using System.Data;
using System.Reflection;
using Mephi.Cybernetics.Race.Core.Attributes;
using Mephi.Cybernetics.Race.Core.UI.Attributes;

namespace Mephi.Cybernetics.Race.DataAccess
{
    [Serializable]
    public abstract class EntityBase : MarshalByRefObject, IEntity, IEntityDescriptor
    {
        public static EntityAttributeDescription Id { get; private set; }

        protected object _ID;
        private EntityState _state = EntityState.New;
        private EntityLoadingState _loadingState = EntityLoadingState.Saved;
        private Dictionary<string, object> _sourceRawValues = new Dictionary<string, object>(new StringCaseInsensitiveComparer());
        private IDataContext _dataContext = null;
        private EntityDescription _entityDescription;

        private ValueCollection _values;

        public delegate void PropertyValueSetDelegate(EntityBase entity, EntityAttributeDescription property, object value);
        public delegate void PropertyValueGetDelegate(EntityBase entity, EntityAttributeDescription property, object value);
        public delegate void PropertyValueChangedDelegate(EntityBase entity, EntityAttributeDescription property, object oldValue, object newValue);
        public delegate void EntityStateChangedDelegate(EntityBase entity, EntityState oldState, EntityState newState);

        public event PropertyValueSetDelegate PropertyValueSet;
        public event PropertyValueGetDelegate PropertyValueGet;
        public event PropertyValueChangedDelegate PropertyValueChanged;
        public event EntityStateChangedDelegate EntityStateChanged;
        public event EventHandler<EntityIdChangedEventArgs> EntityIdChanged = (s, e) =>
        {
        };

        protected EntityBase()
        {
        }

        [Internal]
        public IDataContext DataContext
        {
            get
            {
                return _dataContext;
            }
            set
            {
                _dataContext = value;
            }
        }

        [Internal]
        public DataSource Source
        {
            get;
            set;
        }

        [IdField]
        [DisplayReadOnly]
        [FieldName("id")]
        public object ID
        {
            get
            {
                //if ( _idFieldName == null )
                //{
                //    PrimaryKeyAttribute idattr = OwnerPs.EntitiesFactory.GetObjectDescription(GetType()).GetClrAttribute<PrimaryKeyAttribute>();
                //    if ( idattr != null && idattr.Name != null )
                //        _idFieldName = idattr.Name;
                //    else
                //        _idFieldName = "ID";
                //}
                return GetValue<object>(EntityDescription.GetEntityDescription(GetType()).PrimaryKey.Name);
            }
            set
            {
                //if ( _idFieldName == null )
                //{
                //    PrimaryKeyAttribute idattr = OwnerPs.EntitiesFactory.GetObjectDescription(GetType()).GetClrAttribute<PrimaryKeyAttribute>();
                //    if ( idattr != null && idattr.Name != null )
                //        _idFieldName = idattr.Name;
                //    else
                //        _idFieldName = "ID";
                //}
                var oldValue = GetValue<object>("ID");
                if (oldValue != value)
                {
                    SetValue(EntityDescription.GetEntityDescription(GetType()).PrimaryKey.Name, value);
                    EntityIdChanged(this, new EntityIdChangedEventArgs(oldValue, value));
                }
            }
        }
        //private string _idFieldName = null;

        [Internal]
        public EntityState State
        {
            get
            {
                //return GetValue<EntityState>("State");
                return _state;
            }
            protected set
            {
                //SetValue("State", _state = value);
                if (_state != value)
                {
                    var oldState = _state;
                    _state = value;
                    if (EntityStateChanged != null)
                        EntityStateChanged(this, oldState, value);
                }
            }
        }

        [Internal]
        public EntityLoadingState LoadingState
        {
            get
            {
                return _loadingState;
            }
        }

        [Internal]
        internal object this[int propertyIndex, bool oldValue]
        {
            get { return this[EntityDescriptor.EntityAttributes[propertyIndex], oldValue]; }
            set { this[EntityDescriptor.EntityAttributes[propertyIndex], oldValue] = value; }
        }

        [Internal]
        internal object this[string key, bool oldValue]
        {
            get
            {
                return this[EntityDescriptor.EntityAttributes[key], oldValue];
            }
            set
            {
                this[EntityDescriptor.EntityAttributes[key], oldValue] = value;
            }
        }

        [Internal]
        internal object this[EntityAttributeDescription property, bool oldValue]
        {
            get
            {
                return GetValueInternal(property, oldValue, false);
            }
            set
            {
                if (oldValue)
                {
                    throw new Exception("You can not change the old value of the property");
                }
                else
                {
                    var d = this.EntityDescriptor;
                    if (State != EntityState.Unchanged)
                    {
                        if (PropertyValueChanged != null)
                            PropertyValueChanged(this, property, Values[property.ObjectProperty.Index, true], value);
                    }
                    Values[property.ObjectProperty.Index] = value;
                    //if (!property.IsComputable)
                    foreach (var computableProperty in d.EntityAttributes.ComputableProperties)
                    {
                        if ( property != computableProperty && computableProperty.ObjectProperty.ComputableInfo.AllowCaching && 
                            (computableProperty.ObjectProperty.ComputableInfo.DependencyProperties.Count == 0 || computableProperty.ObjectProperty.ComputableInfo.DependencyProperties.Contains(property.Name)) )
                                Values[computableProperty.ObjectProperty.Index] = new UnasignedValue();
                    }
                    if (PropertyValueSet != null)
                        PropertyValueSet(this, property, value);
                    SetState(EntityState.Changed);
                }
            }
        }

        private object GetValueInternal(EntityAttributeDescription property, bool oldValue, bool callerIsGetValue)
        {
            var val = Values[property.ObjectProperty.Index, oldValue];
            if ( property.IsComputable )
            {
                return val; 
            }
            else
            {
                if (callerIsGetValue && val is UnasignedValue)
                    Values[property.ObjectProperty.Index, oldValue] = val = property.ObjectProperty.PropertyDefaultValue;
            }

            //if (val is UnasignedValue && property.IsNonPersistent && LoadingState != EntityLoadingState.Loading)
            //    val = property.Property.GetAccessors()[0].Invoke(this, null);

            //if ((property.IsManyToManyRelation || property.IsOneToManyRelation) && (val == null || val is UnasignedValue))
            //    if (property.PropertyType.IsGenericType &&
            //        property.PropertyType == typeof(List<>).MakeGenericType(property.PropertyType.GetGenericArguments()[0]))
            //        val = Values[property.Index] = property.PropertyType.GetConstructor(new Type[] { }).Invoke(new object[] { });
            //    else
            //        val = Values[property.Index] = Activator.CreateInstance(property.PropertyType, this, property);


            if (PropertyValueGet != null)
                PropertyValueGet(this, property, val);
            return val;
        }

        [Internal]
        public object this[string key]
        {
            get
            {
                return this[key, false];
            }
            set
            {
                this[key, false] = value;
            }
        }

        [Internal]
        public object this[int propertyIndex]
        {
            get
            {
                return this[propertyIndex, false];
            }
            set
            {
                this[propertyIndex, false] = value;
            }
        }

        [Internal]
        public object this[EntityAttributeDescription property]
        {
            get
            {
                //if (EntityDescriptor == property.ReflectedEntity)
                    return this[property, false];
                //else
                //    throw new ArgumentException("The property does not belong to the object");
            }
            set
            {
                //if (EntityDescriptor == property.ReflectedEntity)
                    this[property, false] = value;
                //else
                //    throw new ArgumentException("The property does not belong to the object");
            }
        }

        protected T GetValue<T>(string name)
        {
            
            var property = EntityDescriptor.EntityAttributes[name];
            var val = GetValueInternal(property, false, true);
            if (val is UnasignedValue)
                return property.ObjectProperty.GetDefaultPropertyValue<T>();
            else
                return (T)val;
        }

        protected T GetValue<T>(EntityAttributeDescription attribute)
        {
            var val = GetValueInternal(attribute, false, true);
            if (val is UnasignedValue)
                return attribute.ObjectProperty.GetDefaultPropertyValue<T>();
            else
                return (T)val;
        }

        protected T GetValue<T>(string name, Fun<T> getter)
        {
            var property = EntityDescriptor.EntityAttributes[name];
            var val = GetValueInternal(property, false, true);
            if ( val is UnasignedValue )
            {
                var result = getter();
                _values[property.ObjectProperty.Index] = result;
                return result;
            }
            else
                return (T)val;
        }

        protected void SetValue(string name, object value)
        {
            this[name] = value;
        }

        [Internal]
        public Dictionary<string, object> SourceRawValues
        {
            get
            {
                return _sourceRawValues;
            }
            set
            {
                _sourceRawValues = value;
            }
        }

        [Internal]
        internal DataRow SourceRow
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public void CreateNew()
        {
            var od = EntityDescription.GetEntityDescription(this.GetType());
            if (od.IdFieldType == typeof(Guid))
                this[od.SystemId.Name] = Guid.NewGuid();
            State = EntityState.New;
        }

        public void Delete()
        {
            SetState(EntityState.Deleted);
        }

        public void Update()
        {
            State = EntityState.Changed;
        }

        [Internal]
        public IEntityDescriptor Description
        {
            get
            {
                return this;
            }
        }

        [Internal]
        public EntityDescription EntityDescriptor
        {
            get
            {
                if (_entityDescription == null)
                    _entityDescription = EntityDescription.GetEntityDescription(this.GetType());
                return _entityDescription;
            }
            set
            {
                _entityDescription = value;
            }
        }

        private ValueCollection Values
        {
            get
            {
                if (_values == null)
                    _values = new ValueCollection(EntityDescriptor);
                return _values;
            }
        }

        public void AcceptChanges()
        {
            Values.AcceptChanges();
            //if ( State != EntityState.Deleted && State != EntityState.NewDeleted )
            State = EntityState.Unchanged;
            _loadingState = EntityLoadingState.Saved;
        }

        public void RejectChanges()
        {
            Values.RejectChanges();
            if (State == EntityState.Changed || State == EntityState.Deleted)
                State = EntityState.Unchanged;
        }

        public void RejectChanges(string propertyName)
        {
            Values.RejectChanges(EntityDescriptor.EntityAttributes[propertyName].ObjectProperty.Index);
            if (Values.CheckUncommitedChanges())
                if (State == EntityState.Changed || State == EntityState.Deleted)
                    State = EntityState.Unchanged;
        }

        internal void BeginLoad()
        {
            if (_loadingState == EntityLoadingState.Saving)
                ;// then what ?!
            _loadingState = EntityLoadingState.Loading;
        }

        internal void EndLoad()
        {
            //State = EntityState.Unchanged;
            //_loadingState = EntityLoadingState.Saved;
            AcceptChanges();
        }

        internal void BeginSave()
        {
            if (_loadingState == EntityLoadingState.Loading)
                ;// then what ?!
            _loadingState = EntityLoadingState.Saving;
        }

        internal void EndSave()
        {
            AcceptChanges();
        }

        internal void SetState(EntityState newState)
        {
            if (State != newState)
            {
                EntityState oldState = State;
                if (newState == EntityState.Deleted)
                {
                    if (State == EntityState.Changed || State == EntityState.Unchanged)
                        this.State = EntityState.Deleted;
                    else if (State == EntityState.New)
                        this.State = EntityState.NewDeleted;
                }
                else if (this.State == EntityState.New)
                {// do nothing
                }
                else
                    this.State = newState;
                // Fire EntityStateChanged event
                if (EntityStateChanged != null)
                    EntityStateChanged(this, oldState, State);
            }
        }
    }

    [Serializable]
    public class ValueCollection
    {
        //private readonly Dictionary<string, ValueContainer> _values = new Dictionary<string, ValueContainer>();
        private readonly ValueContainer[] _values;

        public ValueCollection(EntityDescription entityDescription)
        {
            _values = new ValueContainer[entityDescription.Object.Properties.Count + 1];
            for ( int i = 0; i < entityDescription.Object.Properties.Count + 1; i++ )
            {
                _values[i] = ValueContainer.Empty;
            }
        }

        public int Count
        {
            get
            {
                return _values != null ? _values.Length : 0;
            }
        }

        public object this[int propertyIndex, bool oldVersion]
        {
            get
            {
                //if (_values[propertyIndex] == null)
                //{
                //    _values[propertyIndex] = ValueContainer.Empty;
                //}
                if (oldVersion)
                {
                    var c = _values[propertyIndex];
                    if (c == ValueContainer.Empty || c.OldValue == ValueContainer.Empty)
                        return c.NewValue;

                    return c.OldValue.NewValue;
                }

                return _values[propertyIndex].NewValue;


            }
            set
            {
                //if (_values[propertyIndex] == null)
                //{
                //    _values[propertyIndex] = new ValueContainer(value, ValueContainer.Empty);
                //}
                //else
                {
                    // It should be as follows
                    //ValueContainer old = new ValueContainer(_values[key].NewValue, _values[key].OldValue);
                    //ValueContainer newv = new ValueContainer(value, old);
                    //_values[key] = newv;

                    // but temporarily it would be done in a palliative way
                    var c = new ValueContainer(value, new ValueContainer(_values[propertyIndex].NewValue, ValueContainer.Empty));
                    _values[propertyIndex] = c;
                }
            }
        }

        public bool IsUnassignedValue(int propertyIndex)
        {
            return _values[propertyIndex].IsEmpty;
        }

        public void AcceptChanges()
        {
            for (var index = 0; index < Count; index++)
            {
                if (_values[index] != null)
                    _values[index] = new ValueContainer(_values[index].NewValue, ValueContainer.Empty);
                //_values[key].OldValue = ValueContainer.Empty;
            }
        }

        public void RejectChanges()
        {
            for (var index = 0; index < Count; index++)
            {
                if (_values[index] != null)
                    if (_values[index].OldValue != null)
                        _values[index] = new ValueContainer(_values[index].OldValue.NewValue, _values[index].OldValue.OldValue);
                //_values[key].NewValue = _values[key].OldValue;
                //if (_values[key].OldValue != ValueContainer.Empty)
                //    _values[key].OldValue = _values[key].OldValue.NewValue;
            }
        }

        public void RejectChanges(int propertyIndex)
        {
            _values[propertyIndex] = new ValueContainer(_values[propertyIndex].OldValue.NewValue, _values[propertyIndex].OldValue.OldValue);
            //if (_values[propertyName].OldValue != ValueContainer.Empty)
            //    _values[propertyName].OldValue = _values[propertyName].OldValue.NewValue;
        }

        public bool CheckUncommitedChanges()
        {
            for (var index = 0; index < Count; index++)
            {
                if (_values[index] != null)
                    if (!_values[index].OldValue.IsEmpty)
                        return false;
            }
            return true;
        }

        public object this[int propertyIndex]
        {
            get
            {
                return this[propertyIndex, false];
            }
            set
            {
                this[propertyIndex, false] = value;
            }
        }

        [Serializable]
        public class ValueContainer
        {
            private bool _emptyValue = false;
            private object _newValue;
            private readonly ValueContainer _oldValue;

            public static readonly ValueContainer Empty;

            static ValueContainer()
            {
                Empty = new ValueContainer(new UnasignedValue(), null);
                Empty._emptyValue = true;
            }

            public ValueContainer(object newValue, ValueContainer oldValue)
            {
                _newValue = newValue;
                _oldValue = oldValue;
            }

            public object NewValue
            {
                get
                {
                    return _newValue;
                }
            }

            public ValueContainer OldValue
            {
                get
                {
                    return _oldValue;
                }
            }

            public bool IsEmpty
            {
                get
                {
                    return _emptyValue;
                }
            }
        }
    }

    public class EntityIdChangedEventArgs : EventArgs
    {
        private readonly object _oldId;
        private readonly object _newId;

        public EntityIdChangedEventArgs(object oldId, object newId)
        {
            _oldId = oldId;
            _newId = newId;
        }

        public object NewId
        {
            get { return _newId; }
        }

        public object OldId
        {
            get { return _oldId; }
        }
    }

    public enum EntityLoadingState
    {
        Loading,
        Saved,
        Saving
    }

    public struct UnasignedValue
	{
        public override string ToString()
        {
            return "[<NULL>]";
        }
	}
}
