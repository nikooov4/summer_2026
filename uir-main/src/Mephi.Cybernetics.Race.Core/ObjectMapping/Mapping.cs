using System;
using System.Collections;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core.ObjectMapping
{
    // TODO: Добавить методы, которые позволяли бы описывать свойства преобразования: сохранения монотонности и др. -- необходимо для конвертирования запросов
    /// <summary>
    /// Описывает трансформацию, индексированную типом. В тех случаях, когда исходный, трансформируемый, тип совпадает
    /// с индексом, речь идет об обычных ("абсолютных") преобразованиях (например, для генерации "вида")
    /// TODO: Описание - тупняк, нужно переделать
    /// </summary>
    /// <typeparam name="TSource">Индексирующий тип</typeparam>
    public class Mapping<TSource>
    {
        private static Mapping<TSource> identityMapping;
        private static readonly Dictionary<Type, Mapping<TSource>> genericIdentityMappings;

        static Mapping()
        {
            genericIdentityMappings = new Dictionary<Type, Mapping<TSource>>();
        }

        public static Mapping<TSource> IdentityMapping
        {
            get
            {
                if (identityMapping == null)
                {
                    var od = ObjectDescription.Get(typeof (TSource));
                    identityMapping = new Mapping<TSource>(od, od);
                    identityMapping.UsingIdentityMapping();
                }
                return identityMapping;
            }
        }

        public static Mapping<TSource> GetIdentityMapping(Type type)
        {
            if (!genericIdentityMappings.ContainsKey(type))
            {
                var od = ObjectDescription.Get(type);
                genericIdentityMappings.Add(type, (new Mapping<TSource>(od, od)).UsingIdentityMapping());
            }
            return genericIdentityMappings[type];
        }

        private readonly MappingCollection<TSource> _targetProperties;
        private readonly Dictionary<Type,MappingCollection<TSource>> _extensionMappings;
        private readonly ObjectDescription _targetEntityDescription;
        private readonly ObjectDescription _sourceEntityDescription;
        private readonly MappingDirection _direction;
        private Func<TSource, object> _mapping;
        private Func<TSource,object> _objectInstantiation;
        private PropertyDescription[] _dependencies;
        private AutoUpdateEvent _autoUpdateMode;

        public Mapping(ObjectDescription targetEntityDescription, MappingDirection direction)
            : this(ObjectDescription.Get(typeof(TSource)), targetEntityDescription)
        {
        }

        public Mapping(ObjectDescription targetEntityDescription) : this(targetEntityDescription, MappingDirection.Covariant)
        {
        }

        public Mapping(Type targetType, MappingDirection direction)
            : this(ObjectDescription.Get(typeof(TSource)), ObjectDescription.Get(targetType), direction)
        {
        }

        public Mapping(Type targetType) : this(targetType, MappingDirection.Covariant)
        {
        }

        public Mapping(ObjectDescription source, ObjectDescription target) : this(source, target, MappingDirection.Covariant)
        {
        }

        public Mapping(ObjectDescription source, ObjectDescription target, MappingDirection direction)
        {
            if (!source.IsSubtypeOf(ObjectDescription.Get(typeof(TSource))))
                throw new ArgumentException("!!!");
            _autoUpdateMode = AutoUpdateEvent.Never;
            _sourceEntityDescription = source;
            _targetEntityDescription = target;
            _objectInstantiation = TrivialObjectInstantiation;
            _direction = direction;
            _targetProperties = new MappingCollection<TSource>(_sourceEntityDescription, _targetEntityDescription);
            _extensionMappings = new Dictionary<Type, MappingCollection<TSource>>();
        }

        public ObjectDescription SourceEntity
        {
            get { return _sourceEntityDescription; }
        }

        public ObjectDescription TargetEntity
        {
            get { return _targetEntityDescription; }
        }

        public MappingDirection Direction
        {
            get { return _direction; }
        }

        public AutoUpdateEvent AutoUpdateMode
        {
            get { return _autoUpdateMode; }
        }

        public MappingCollection<TSource> TargetProperties
        {
            get { return _targetProperties; }
        }

        #region Standard transformations

        public Mapping<TSource> UsingIdentityMapping()
        {
            if ( _targetEntityDescription.ClrType != _sourceEntityDescription.ClrType )
                throw new InvalidOperationException("Identity transformation required the target type being the same as the source");
            TargetProperties.Clear();

            if ( _sourceEntityDescription.IsAssumedPrimitive )
            {
                _mapping = e => e;
            }
            else
            {
                foreach ( var property in _targetEntityDescription.Properties )
                {
                    if ( property.IsComputable || property.IsReadOnly )
                        continue;
                    if ( property.Range.IsSubtypeOf(typeof(IList)) )
                    {
                        var p = property;
                        TargetProperties[p].UsingMap(o =>
                                                                {
                                                                    var list = p.GetDefaultPropertyValue() as IList;
                                                                    foreach ( var value in p.GetValue(o) as IList )
                                                                    {
                                                                        list.Add(value);
                                                                    }
                                                                    return list;
                                                                }).DependsOn(p.Name);
                    }
                    else
                    {
                        var property1 = property; // OMFG!!! Либо я полный идиот, либо разрабы дотнета конченые дауны.... КАК??!?!??!!???
                        TargetProperties[property].UsingMap(o => property1.GetValue(o));
                            // и какого хрена ковариантность/контравариантность не работает ???
                    }
                }
                _mapping = PropertyMappingTransformation;
            }
            return this;
        }

        /// <summary>
        /// Sets a custom map function that will not be managed by the current mapping (depencies will be reset and auto-updates will be disabled);
        /// MapOnto method will be ineffectual
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="func">The actual map function</param>
        /// <returns></returns>
        public Mapping<TSource> UsingMap<TTarget>(Func<TSource, TTarget> func)
        {
            this._mapping = a => func(a);
            TargetProperties.Clear();
            _autoUpdateMode = AutoUpdateEvent.Never;
            return this;
        }

        private object PropertyMappingTransformation(TSource source)
        {
            var result = _objectInstantiation(source);
            //
            // This is the place to set auto-update handlers
            // Presently this logic is possible to write for EntityBase-derived classes only 
            //
            return MapOnto(source, ref result);
        }

        #endregion

        #region Standard object initializations
        private object TrivialObjectInstantiation(TSource sourceObject)
        {
            return _targetEntityDescription.CreateObject();
        }

        /// <summary>
        /// Will instantiate the object using the default (parameterless) overload of the EntityDescription.CreateObject method
        /// </summary>
        /// <returns>this same mapping</returns>
        public Mapping<TSource> UsingDefualtInstantiation()
        {
            _objectInstantiation = TrivialObjectInstantiation;
            _mapping = PropertyMappingTransformation;
            return this;
        }

        public Mapping<TSource> UsingCustomInstantiation(Func<TSource,object> constructor)
        {
            _objectInstantiation = constructor;
            _mapping = PropertyMappingTransformation;
            return this;
        }
        #endregion 

        public object Map(TSource source)
        {
            return _mapping(source);
        }

        public List<object> Map(ICollection<TSource> objs)
        {
            var list = new List<object>(objs.Count);
            foreach (TSource obj in objs)
                list.Add(Map(obj));
            return list;
        }

        /// <summary>
        /// Updates the target object by mapping on it the source objects and its properties, without creating a new target object;
        /// the target object will remian unchanched if a custom map has been set with the UsingMap method
        /// </summary>
        /// <param name="source">The object to be projected</param>
        /// <param name="target">The object on which the source is being mapped</param>
        /// <returns>The target object</returns>
        public object MapOnto(TSource source, ref object target)
        {
            var od = ObjectDescription.Get(target.GetType());
            return MapOntoInner(source, ref target, od);
        }

        private object MapOntoInner(TSource source, ref object target, ObjectDescription od)
        {
            foreach ( var mapping in TargetProperties )
                mapping.TargetProperty.SetValue(target, mapping.Map(source));
            foreach ( var extension in _extensionMappings.Values )
                if ( od.IsSubtypeOf(extension.TargetEntityDescription) )
                    foreach ( var mapping in extension )
                        if ( mapping.TargetProperty.Domain.IsSubtypeOf(od) )
                            mapping.TargetProperty.SetValue(target, mapping.Map(source));
            return target;
        }

        /// <summary>
        /// Sets mapping's direct dependencies which will trigger the mapping if AutoUpdate property value is OnValueSet or OnAcceptChanges
        /// </summary>
        /// <param name="properties">the list of names of source's properties, which values changing will trigger the mapping</param>
        /// <returns>this same mapping</returns>
        public Mapping<TSource> DependsOn(params string[] properties)
        {
            return SetDependencies(properties);
        }

        private Mapping<TSource> SetDependencies(string[] properties)
        {
            _dependencies = new PropertyDescription[properties.Length];
            for ( int i = 0; i < properties.Length; i++ )
                _dependencies[i] = _sourceEntityDescription.Properties[properties[i]];
            //_isPersistent = true;
            return this;
        }

        public MappingCollection<TSource> ForType<TType>()
        {
            return ForType(typeof (TType));
        }

        public MappingCollection<TSource> ForType(Type type)
        {
            var od = ObjectDescription.Get(type);
            if ( od.IsSubtypeOf(_targetEntityDescription) )
            {
                if ( !_extensionMappings.ContainsKey(type) )
                    _extensionMappings.Add(type, new MappingCollection<TSource>(_sourceEntityDescription, od));
                return _extensionMappings[type];
            }
            else
                throw new ArgumentException(string.Format("The type '{0}' is not subtype if the type '{1}'", type.Name, _targetEntityDescription.ClrType.Name));
        }

        /// <summary>
        /// Sets mapping's direct dependencies which will trigger the mapping if AutoUpdate property value is OnValueSet or OnAcceptChanges
        /// </summary>
        /// <param name="properties">the list of source's properties, which values changing will trigger the mapping</param>
        /// <exception cref="ArgumentException">if a property in the providedlist does not belong to the source type</exception>
        /// <returns>this same mapping</returns>
        public Mapping<TSource> DependsOn(params PropertyDescription[] properties)
        {
            var propertyNames = new List<string>();
            foreach ( var property in properties )
                if ( property.Domain.IsSubtypeOf(_sourceEntityDescription) )
                    propertyNames.Add(property.Name);
                else
                    throw new ArgumentException(string.Format("The given property '{0}' does not belong to the type '{1}'", property, _sourceEntityDescription));
            //_isPersistent = true;
            return SetDependencies(propertyNames.ToArray());
        }

        public Mapping<TSource> AutoUpdate(AutoUpdateEvent mode)
        {
            _autoUpdateMode = mode;
            return this;
        }

        public Mapping<TSource> ManualUpdate()
        {
            _autoUpdateMode = AutoUpdateEvent.Never;
            return this;
        }

        public static explicit operator Func<TSource, object>(Mapping<TSource> transformation)
        {
            return transformation.Map;
        }
    }

    public enum AutoUpdateEvent
    {
        OnValueSet,
        OnAcceptChanges,
        Never
    }

    public enum MappingDirection
    {
        Covariant,
        Contravariant
    }

    public class Mapping<TSource, TTarget> : Mapping<TSource>
    {

        private Func<TSource, TTarget> _transformation;

        public Mapping() : base(typeof(TTarget))
        {
        }

        //public Transformation<TSource> CustomValuation(Fun<TSource, TTarget> func)
        //{
        //    return base.UsingMap(func);
        //}

        public TTarget Convert(TSource source)
        {
            return (TTarget)Map(source);
        }

        public List<TTarget> Convert(ICollection<TSource> objs)
        {
            var list = new List<TTarget>(objs.Count);
            foreach (TSource obj in objs)
                list.Add(Convert(obj));
            return list;
        }

        public TTarget MapOnto(TSource source, ref TTarget target)
        {
            var oTarget = (object)target; // omg...
            return (TTarget)base.MapOnto(source, ref oTarget);
        }

        public static explicit operator Func<TSource,TTarget>(Mapping<TSource, TTarget> mapping)
        {
            return mapping.Convert;
        }
    }

    /// <summary>
    /// Этот класс предназначен для определения преобразований в динамическом контексте, когда преобразуемые типы не известны на этапе компиляции
    /// </summary>
    public class Mapping : Mapping<object>
    {
        private static readonly Dictionary<Type, Mapping> identityMappings;

        static Mapping()
        {
            identityMappings = new Dictionary<Type, Mapping>();
        }

        public static Mapping GetIdentityMapping(Type type)
        {
            if (!identityMappings.ContainsKey(type))
            {
                var od = ObjectDescription.Get(type);
                var m = new Mapping(od, od);
                m.UsingIdentityMapping();
                identityMappings.Add(type, m);
            }
            return identityMappings[type];
        }

        public Mapping(ObjectDescription sourceEntity, ObjectDescription targetEntity)
            : base(sourceEntity, targetEntity)
        {

        }
    }

}