using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UniqueFieldsetConstraintAttribute : EntityConstraintAttribute
    {
        private readonly bool _allowEmptyValues;
        private readonly string[] _fields;
        private readonly string _errorMessage;

        public UniqueFieldsetConstraintAttribute([Optional, DefaultParameterValue(false)] bool AllowEmptyValues, params string[] fields)
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentException("At least one field should be specified");
            _allowEmptyValues = AllowEmptyValues;
            _fields = fields;
        }

        public UniqueFieldsetConstraintAttribute([Optional, DefaultParameterValue(false)] bool AllowEmptyValues, string errorMessage, params string[] fields)
        {
            if (fields == null || fields.Length == 0)
                throw new ArgumentException("At least one field should be specified");
            _allowEmptyValues = AllowEmptyValues;
            _fields = fields;
            _errorMessage = errorMessage;
        }

        public bool AllowEmptyValues
        {
            get { return _allowEmptyValues; }
        }

        public override string ErrorMessage
        {
            get
            {
                if (string.IsNullOrEmpty(_errorMessage))
                {
                    var fieldNames = "";
                    foreach (var field in _fields)
                    {
                        if (string.IsNullOrEmpty(fieldNames))
                            fieldNames = field;
                        else
                            fieldNames += ", " + field;
                    }
                    return string.Format("Набор значений свойств {0} должен быть уникальным", fieldNames);
                }
                else 
                    return _errorMessage;
            }
        }

        public override bool CheckEntity(EntityBase entity)
        {
            if (entity.DataContext == null)
                return true;
            var list = entity.DataContext.GetEntitiesByPredicate(entity.GetType(),
                                                             (e) =>
                                                                 {
                                                                     var satisfy = true;
                                                                     foreach (var field in _fields)
                                                                         satisfy = satisfy && AreEqual(e[field], entity[field]);
                                                                     return satisfy;
                                                                 });
            return list.Count == 1 && list[0] == entity || list.Count == 0;
        }

        public override bool CheckEntity(IEntity entity, Dictionary<string, object> values)
        {
            if (entity.Description.DataContext == null)
                return true;
            var valuesAreSet = true;
            foreach (var field in _fields)
                valuesAreSet = valuesAreSet && AreEqual(entity[field], values[field]);
            var list = entity.Description.DataContext.GetEntitiesByPredicate(entity.GetType(),
                                                             (e) =>
                                                             {
                                                                 var satisfy = true;
                                                                 foreach (var field in _fields)
                                                                     satisfy = satisfy && AreEqual(e[field], values[field]);
                                                                 return satisfy;
                                                             });
            if (!AllowEmptyValues)
                return list.Count == 1 && list[0] == entity && valuesAreSet || list.Count == 0;
            else
            {
                var thereAreNullValues = false;
                foreach (var field in _fields)
                    if (values[field] == null || values[field] is string && string.IsNullOrEmpty(values[field] as string))
                    {
                        thereAreNullValues = true;
                        break;
                    }
                return thereAreNullValues || list.Count == 1 && list[0] == entity && valuesAreSet || list.Count == 0;
            }
        }

        private bool AreEqual(object o1, object o2)
        {
            return AreEqual(o1, o2, true);
        }

        private bool AreEqual(object o1, object o2, bool ignoreAllowNullValues)
        {
            //if (_allowEmptyValues && !ignoreAllowNullValues)
            //    return o1 == null && o2 == null || o1 != null && o1.Equals(null) || o2 != null && o2.Equals(null);
            //else
            //    return o1 == null && o2 == null || o1.Equals(o2);
            return o1 != null && o1.Equals(o2) || o1 == null && o2 == null;
        }
    }
}
