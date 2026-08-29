using System;
using System.Collections;
using System.Collections.Generic;
//using Mephi.Cybernetics.Race.DataAccess;
//using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.Core
{
    //TODO: вернуть internal
    public class AttributeContainer : IEnumerable<Attribute>
    {
        private readonly ValueCell<Attribute>[] _attributes;
        private readonly Attribute[] _attributesList;

        public AttributeContainer(object[] attributes)
        {
            _attributes = new ValueCell<Attribute>[attributes.Length];
            _attributesList = new Attribute[attributes.Length];
            for ( var i = 0; i < attributes.Length; i++ )
            {
                _attributesList[i] = attributes[i] as Attribute;
                //_attributes[i] = new ValueCell<Attribute>(attributes[i] as Attribute);
            }
        }

        /// <summary>
        /// Возвращает атрибут (тип которого производный от указанного класса), примененный к данному типу.
        /// Если таких окажется несколько, выбирается "наиболее конкретный". 
        /// "Нативная" реализация выбрасывает исключение, не пытаясь разрешать неоднозначности
        /// (В теории, эта реализация должна работать быстрее, чем "нативная" Type.GetCustomAttribute,
        /// за счет кеширования)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Attribute GetAttribute(Type type)
        {
            PropertyDescription.Func<Type, int> getDistanceFromSupertype = child =>
                                                                           {
                                                                               var count = -1;
                                                                               for ( var t = child; t != null; t = t.BaseType )
                                                                               {
                                                                                   count++;
                                                                                   if ( type == t )
                                                                                       return count;
                                                                               }
                                                                               return -1;
                                                                           };

            Attribute farthestAttribute = null;
            var attributeDistance = -1;
            foreach ( var attr in _attributesList )
            {
                var distance = getDistanceFromSupertype(attr.GetType());
                if ( distance > attributeDistance )
                {
                    attributeDistance = distance;
                    farthestAttribute = attr;
                }
            }
            return farthestAttribute;
        }

        public bool HasAttribute(Type attributeType)
        {
            return this.GetAttribute(attributeType) != null;
        }

        /// <summary>
        /// Возвращает атрибут (тип которого производный от указанного класса), примененный к данному типу.
        /// Если таких окажется несколько, выбирается "наиболее конкретный".
        /// "Нативная" реализация выбрасывает исключение, не пытаясь разрешать неоднозначности
        /// (В теории, эта реализация должна работать на несколько порядков быстрее, чем "нативная" Type.GetCustomAttribute,
        /// за счет кеширования)
        /// </summary>
        /// <returns></returns>
        public T GetAttribute<T>() where T : Attribute
        {
            return GetAttribute(typeof(T)) as T;
        }

        public bool HasAttribute<T>() where T : Attribute
        {
            return HasAttribute(typeof(T));
        }

        /// <summary>
        /// Возвращает все атрибуты, примененные к данному типу, производные от указанного класса.
        /// В теории, эта реализация должна работать быстрее, чем "нативная" Type.GetCustomAttributes
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public IList<Attribute> GetAttributes(Type attributeType)
        {
            throw new NotImplementedException();
//            var list = new List<Attribute>();
//            foreach (var attribute in _attributes)
//                if (attribute.MetaInfo.IsSubtypeOf(attributeType))
//                    list.Add(attribute);
//            return list;
        }

        /// <summary>
        /// Возвращает все атрибуты, примененные к данному типу, производные от указанного класса.
        /// В теории, эта реализация должна работать быстрее, чем "нативная" Type.GetCustomAttributes
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public IList<Attribute> GetAttributes<T>()
        {
            return GetAttributes(typeof(T));
        }


        private struct ValueCell<T>
        {
            public ValueCell(T value)
            {
                Value = value;
                MetaInfo = ObjectDescription.Get(value.GetType());
            }

            public T Value;
            public ObjectDescription MetaInfo;

            public static implicit operator T(ValueCell<T> cell)
            {
                return default(T);
            }
        }

        public IEnumerator<Attribute> GetEnumerator()
        {
            return new SwitchableEnumerator<Attribute>(_attributesList);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}