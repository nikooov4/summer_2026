using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueConstraintAttribute : FieldConstraintAttribute
    {
        private readonly string _errorMessage;

        public UniqueConstraintAttribute()
        {
            
        }

        public UniqueConstraintAttribute(string errorMessage)
        {
            _errorMessage = errorMessage;
        }

        public override bool CheckValue(IEntity entity, EntityAttributeDescription property, object value)
        {
            if (entity.Description.DataContext == null)
                return true;
            var list = entity.Description.DataContext.GetEntitiesByFieldValue(entity.GetType(), property.Name, value);
            if (entity[property] != value)
                return list.Count == 0;
            else
                return list.Count == 1 && list[0] == entity;
        }

        public override bool CheckValue(EntityBase entity, EntityAttributeDescription property)
        {
            if (entity.DataContext == null)
                return true;
            var list = entity.DataContext.GetEntitiesByFieldValue(entity.GetType(), property.Name, property.GetValue(entity));
            return list.Count == 1 && list[0] == entity || list.Count == 0;
        }

        public override string ErrorMessage
        {
            get
            {
                if (string.IsNullOrEmpty(_errorMessage))
                    return "Значение свойства должно быть уникальным";
                else
                    return _errorMessage;
            }
        }
    }
}
