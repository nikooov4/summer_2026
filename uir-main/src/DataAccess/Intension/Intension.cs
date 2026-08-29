using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.Intension
{
    public abstract class Entity
    {
        public static bool operator <= (Entity e1, Entity e2)
        {
            throw new NotImplementedException();
        }

        public static bool operator >=(Entity e1, Entity e2)
        {
            throw new NotImplementedException();
        }

        public static bool operator <(Entity e1, Entity e2)
        {
            throw new NotImplementedException();
        }

        public static bool operator >(Entity e1, Entity e2)
        {
            throw new NotImplementedException();
        }

        public abstract Entity[] Parents { get; }

        public Dictionary<string, Property> Properties
        {
            get { return null; }
        }

        public Dictionary<string, Method> Methods
        {
            get { return null; }
        }

        public abstract bool IsSubtypeOf(Entity entity);

    }

    public abstract class EntityMember
    {

        public Entity Domain
        {
            get { return null; }
        }
    }

    public abstract class Method : EntityMember
    {
        public Entity[] Signature
        {
            get { return null; }
        }

        public Entity[] ArgumentTypes
        {
            get { return null; }
        }

        public Entity ReturnType
        {
            get { return null; }
        }

        public Instance Evaluate(Instance[] arguments)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Property : EntityMember
    {


        public Entity Range
        {
            get { return null; }
        }

        public abstract object GetValue(Instance instance);
    }

    public class SimpleProperty : Property
    {
        public override object GetValue(Instance instance)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class ComputableProperty : Property
    {
        
    }

    public class Instance
    {
        private readonly InstancePropertyValueList _propertiesValues;

        public Instance()
        {
            _propertiesValues = new InstancePropertyValueList(this);
        }

        public Entity Domain
        {
            get { return null; }
        }

        public InstancePropertyValueList PropertiesValues
        {
            get { return _propertiesValues; }
        }
    }

    public class InstancePropertyValueList : IEnumerable<KeyValuePair<Property,Instance>>
    {
        private readonly Instance _instance;

        public InstancePropertyValueList(Instance instance)
        {
            _instance = instance;
            throw new NotImplementedException();
        }

        public Instance this [Property property]
        {
            get { return null; }
        }

        public Instance this [string propertyName]
        {
            get { return null; }
        }

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<Property, Instance>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
