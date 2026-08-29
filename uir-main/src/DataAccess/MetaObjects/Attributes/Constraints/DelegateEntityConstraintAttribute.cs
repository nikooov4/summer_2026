using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DelegateEntityConstraintAttribute : EntityConstraintAttribute
    {
        private string _errorMessage;
        private MethodInfo _checkEntity1;
        private MethodInfo _checkEntity2;

        public DelegateEntityConstraintAttribute(string className, string methodName)
        {
            var type = Type.GetType(className, true);
            _checkEntity1 = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, 
                new Type[] {typeof(EntityBase)}, new ParameterModifier[] {});
            _checkEntity2 = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, 
                new Type[] { typeof(EntityBase), typeof(Dictionary<string, object>) }, new ParameterModifier[] { });
            if (_checkEntity1 == null || _checkEntity2 == null)
                throw new ArgumentException(string.Format("Could not find public static methods '{0}' in the class '{1}'", className, methodName));
        }

        public DelegateEntityConstraintAttribute(string className, string methodName, string  errorMessage) : this(className, methodName)
        {
            _errorMessage = errorMessage;
        }

        public override string ErrorMessage
        {
            get
            {
                if (string.IsNullOrEmpty(_errorMessage))
                    return string.Format("Не выполнено ограничение, заданное методом '{0}' в классе '{1}'", _checkEntity1.Name, _checkEntity1.ReflectedType);
                else
                    return _errorMessage;
            }
        }

        public override bool CheckEntity(EntityBase entity)
        {
            if (!_checkEntity1.ReflectedType.IsAssignableFrom(entity.GetType()))
                throw new ArgumentException(string.Format("Expected an object of type '{0}' but a '{1}' instance given", _checkEntity1.ReflectedType, entity.GetType()));
            return (bool)_checkEntity1.Invoke(null, new object[] { entity });
        }

        public override bool CheckEntity(IEntity entity, Dictionary<string, object> values)
        {
            if (!_checkEntity2.ReflectedType.IsAssignableFrom(entity.GetType()))
                throw new ArgumentException(string.Format("Expected an object of type '{0}' but a '{1}' instance given", _checkEntity1.ReflectedType, entity.GetType()));
            return (bool)_checkEntity2.Invoke(null, new object[] { entity, values });
        }
    }
}
