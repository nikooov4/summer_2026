using System;

namespace Mephi.Cybernetics.Race.Core.ObjectMapping
{
    public class PropertyMapping<TSource> : Mapping<TSource>
    {
        private readonly PropertyDescription _targetProperty;

        /// <summary>
        /// Создает индексированное преобразование из типа одного типа в другой относительно типа-индекса: свойство source преобразуется в свойство target
        /// </summary>
        /// <param name="sourceProperty">Свойство, принадлежащее исходному типу и область значения которого требует преобразования</param>
        /// <param name="targetProperty">Свойство, принадлежащее целевому типу</param>
        /// <param name="direction">Напрвление преобразования: ковариантное или контравариантное</param>
        public PropertyMapping(PropertyDescription sourceProperty, PropertyDescription targetProperty, MappingDirection direction)
            : this(targetProperty, direction)
        {
            DependsOn(sourceProperty);
        }

        public PropertyMapping(PropertyDescription sourceProperty, PropertyDescription targetProperty)
            : this(sourceProperty, targetProperty, MappingDirection.Covariant)
        {
        }

        public PropertyMapping(PropertyDescription targetProperty, MappingDirection direction)
            : base(ObjectDescription.Get(targetProperty.PropertyType), direction)
        {

            _targetProperty = targetProperty;
        }

        public PropertyMapping(PropertyDescription targetProperty)
            : this(targetProperty, MappingDirection.Covariant)
        {

        }
        
        /// <summary>
        /// Для тех случаев, когда TSource не является SourceEntity'ом, например, когда приходится описывать преобразования в динамическом контексте
        /// </summary>
        /// <param name="sourceEntityDescription"></param>
        /// <param name="targetProperty"></param>
        public PropertyMapping(ObjectDescription sourceEntityDescription, PropertyDescription targetProperty)
            : base(sourceEntityDescription, targetProperty.Range)
        {
            _targetProperty = targetProperty;
        }

        public PropertyDescription TargetProperty
        {
            get
            {
                return _targetProperty;
            }
        }

        public Mapping<TSource> FromProperty<TSourcePropertyRange, TTargetPropertyRange>(string sourcePropertyName, Mapping<TSourcePropertyRange> map)
        {
            return FromProperty<TSourcePropertyRange,TTargetPropertyRange>(SourceEntity.Properties[sourcePropertyName], map);
        }

        public Mapping<TSource> FromProperty<TSourcePropertyRange, TTargetPropertyRange>(PropertyDescription sourceProperty, Mapping<TSourcePropertyRange> map)
        {
            if ( !TargetEntity.ClrType.IsAssignableFrom(map.TargetEntity.ClrType) )
                throw new ArgumentException("'map's return type is incompatible with the target property type");
            if ( !SourceEntity.Properties.Contains(sourceProperty) )
                throw new ArgumentException(string.Format("The type '{0}' does not contain the property '{1}'", SourceEntity.ClrType.Name, sourceProperty));
            if ( !typeof(TSourcePropertyRange).IsAssignableFrom(sourceProperty.PropertyType) )
                throw new ArgumentException("'map's argument type is incompatible with the source property type");
            
            DependsOn(sourceProperty);
            UsingMap(o => map.Map((TSourcePropertyRange)sourceProperty.GetValue(o)));
            return this;
        }

        public Mapping<TSource> FromProperty<TSourcePropertyRange, TTargetPropertyRange>(PropertyDescription sourceProperty, Func<TSourcePropertyRange, TTargetPropertyRange> mapping)
        {
            if ( !TargetEntity.ClrType.IsAssignableFrom(typeof(TTargetPropertyRange)) )
                throw new ArgumentException("'map's return type is incompatible with the target property type");
            if ( !SourceEntity.Properties.Contains(sourceProperty) )
                throw new ArgumentException(string.Format("The type '{0}' does not contain the property '{1}'", SourceEntity.ClrType.Name, sourceProperty));
            if ( !typeof(TSourcePropertyRange).IsAssignableFrom(sourceProperty.PropertyType) )
                throw new ArgumentException("'map's argument type is incompatible with the source property type");

            DependsOn(sourceProperty);
            UsingMap(o => mapping((TSourcePropertyRange)sourceProperty.GetValue(o)));
            return this;
        }

        public Mapping<TSource> FromProperty<TSourcePropertyRange, TTargetPropertyRange>(string sourcePropertyName, Func<TSourcePropertyRange, TTargetPropertyRange> map)
        {
            return FromProperty(SourceEntity.Properties[sourcePropertyName], map);
        }

        public Mapping<TSource> FromProperty(PropertyDescription sourceProperty, Mapping<object> map)
        {
            if ( !SourceEntity.Properties.Contains(sourceProperty) )
                throw new ArgumentException(string.Format("The type '{0}' does not contain the property '{1}'", SourceEntity.ClrType.Name, sourceProperty));
            if (!SourceEntity.IsSubtypeOf(map.SourceEntity)) // ковариантность/контравариантность в действии
                throw new ArgumentException("'map's SourceEntity is not a super-type of the current mapping's SourceEntity");
            if (!map.TargetEntity.IsSubtypeOf(TargetEntity)) // ковариантность/контравариантность опять в действии, ура!!
                throw new ArgumentException("'map's TargetEntity is not a sub-type of the current mapping's TargetEntity");

            DependsOn(sourceProperty);
            UsingMap(o => map.Map(sourceProperty.GetValue(o)));
            return this;
        }
    }
}