using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Mephi.Cybernetics.Race.Core;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DelegateFieldConstraintAttribute : FieldConstraintAttribute
    {
        private string _errorMessage;
        private MethodInfo _checkEntity1;
        private MethodInfo _checkEntity2;

        public DelegateFieldConstraintAttribute(string className, string methodName)
        {
            var type = Type.GetType(className, true);
            _checkEntity1 = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null,
                new Type[] { typeof(PropertyDescription), typeof(object) }, new ParameterModifier[] { });
            _checkEntity2 = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null,
                new Type[] { typeof(PropertyDescription) }, new ParameterModifier[] { });
            if (_checkEntity1 == null || _checkEntity2 == null)
                throw new ArgumentException(string.Format("Could not find public static methods '{0}' in the class '{1}'", className, methodName));
        }

        public DelegateFieldConstraintAttribute(string className, string methodName, string errorMessage)
            : this(className, methodName)
        {
            _errorMessage = errorMessage;
        }

        public override string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
        }

        public override bool CheckValue(IEntity entity, EntityAttributeDescription property, object value)
        {
            if (!_checkEntity1.ReflectedType.IsAssignableFrom(entity.GetType()))
                throw  new ArgumentException(string.Format("Expected an object of type '{0}' but a '{1}' instance given", _checkEntity1.ReflectedType, entity.GetType()));
            return (bool)_checkEntity1.Invoke(entity, new object[] { property, value });
        }

        public override bool CheckValue(EntityBase entity, EntityAttributeDescription property)
        {
            if (!_checkEntity2.ReflectedType.IsAssignableFrom(entity.GetType()))
                throw new ArgumentException(string.Format("Expected an object of type '{0}' but a '{1}' instance given", _checkEntity2.ReflectedType, entity.GetType()));
            return (bool)_checkEntity2.Invoke(entity, new object[] { property });
        }
    }
}
