using Mephi.Cybernetics.Race.Core.TypeSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Mephi.Cybernetics.Race.Core.DomainModelMapping
{
    public class TypePropertyMapping<TSource, TDestination> where TSource : IDomain, IDomainAttributesInfo, IDomainSubtypingInfo where TDestination : IDomain, IDomainAttributesInfo, IDomainSubtypingInfo
    {
        public string DestinationPropertyName { get; private set; }
        internal Func<TDestination, IDomainAttribute> DestinationPropertySelector { get; private set; }
        internal Func<TSource, object, object> MappedValueSelector { get; private set; }
        protected TSource _sourceType;
        protected TDestination _destinationType;

        internal TypePropertyMapping(string destinationPropertyName, TSource sourceType, TDestination destinationType)
        {
            _sourceType = sourceType;
            _destinationType = destinationType; 
            DestinationPropertySelector = (dest) => dest.Properties[destinationPropertyName];
            DestinationPropertyName = destinationPropertyName;
        }

        public TypePropertyMapping(string sourcePropertyName, string destinationPropertyName, TSource sourceType, TDestination destinationType) : this(destinationPropertyName, sourceType, destinationType)
        {
            FromProperty(sourcePropertyName);
        }

        public TypePropertyMapping<TSource, TDestination> FromProperty(string name)
        {
            MappedValueSelector = (sourceType, sourceInstance) =>
            {
                return sourceType.Properties[name].GetValue(sourceInstance);   
            };
            return this;
        }

        public TypePropertyMapping<TSource, TDestination> FromProperty<SPropety, DProperty>(string name, Func<SPropety, DProperty> map)
        {
            MappedValueSelector = (sourceType, sourceInstance) =>
            {
                var sourceValue = sourceType.Properties[name].GetValue(sourceInstance);
                return map((SPropety)sourceValue);
            };
            return this;
        }

        private void MapOnto(TSource sourceType, object source, TDestination destType, object dest)
        {
            var dProperty = DestinationPropertySelector(destType);
            var dValue = MappedValueSelector(sourceType, source);
            dProperty.SetValue(dest, dValue);
        }

         public void UsingMap<S, D>(Func<S,D> map)
        {
            MappedValueSelector = (sourceType, s) => map((S)s);
        }

        public object Map<S>(S sourceObject)
        {
            return MappedValueSelector(_sourceType, sourceObject);
        }

        public void MapOnto<S, D>(S o, D res)
        {
            MapOnto(_sourceType, o, _destinationType, res);
        }
    }



    public class InterMapping<TSource, TDestination> where TSource : class, IDomain, IDomainAttributesInfo, IDomainSubtypingInfo where TDestination : class, IDomain, IDomainAttributesInfo, IDomainSubtypingInfo
    {
        protected Func<object, object> _initInstance; // source instance to destination instance
        public TSource SourceType { get; protected set; }
        public TDestination DestinationType { get; protected set; }

        protected Dictionary<string, TypePropertyMapping<TSource, TDestination>> _propertyMappings;
        protected List<InterMapping<TSource, TDestination>> _subtypeMappings;

        protected Func<TSource, object, TDestination, object> _map { get; private set; }

        protected InterMapping()
        {
            _propertyMappings = new();
            _subtypeMappings = new();
        }

        protected InterMapping(TSource sourceType, TDestination destinationType) : this()
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }

        public static InterMapping<TSource, TDestination> New(TSource sourceType, TDestination destinationType)
        {
            var res = new InterMapping<TSource, TDestination>(sourceType, destinationType);
            return res;
        }

        public virtual InterMapping<TSource, TDestination> ForType(TDestination type)
        {
            if (type.IsSubtypeOf(DestinationType))
            {
                var mapping = Utils.FirstOrDefault(_subtypeMappings, m => m.DestinationType.FullName == type.FullName);
                if (mapping != default)
                {
                    mapping = New(SourceType, type);
                    _subtypeMappings.Add(mapping);
                }
                return mapping;
            }
            else
                return null;

        }

        public InterMapping<TSource, TDestination> UsingCustomInstantiation<S, D>(Func<S, D> init)
        {
            _initInstance = (object sourceInstance) => init((S)sourceInstance);
            return this;
        }   

        public TypePropertyMapping<TSource, TDestination> Property(string destinationPropertyName)
        {
            if (!_propertyMappings.TryGetValue(destinationPropertyName, out var mapping))
            {
                mapping = new TypePropertyMapping<TSource, TDestination>(destinationPropertyName, SourceType, DestinationType);
                _propertyMappings.Add(destinationPropertyName, mapping);
            }
            return mapping;
        }

        public InterMapping<TSource, TDestination> CustomMapping<S, D>(Func<S, D> map)
        {
            _map = (sType, s, dType) => map((S)s);
            return this;
        }

        public D Map<S, D>(S sourceObject)
        {
            return Map<S, D>(sourceObject, DestinationType);
        }

        public D Map<S, D>(S sourceObject, TDestination destType)
        {
            return Map<S, D>(sourceObject, destType, default);
        }

        private static bool IsDefault<T>(T o) => (o == null) ? (default(T) == null) : o.Equals(default(T));

        protected D Map<S, D>(S sourceObject, TDestination destType, D pureDestinationObj)
        {
            if(_initInstance == null)
            {
                _initInstance = (s) => Activator.CreateInstance<D>();
            }

            if (destType == DestinationType)
            {
                if (_map != default)
                {
                    return (D)_map(SourceType, sourceObject, DestinationType);
                }
                else
                {
                    if (IsDefault(pureDestinationObj))
                        pureDestinationObj = (D)_initInstance(sourceObject);
                    MapOnto(sourceObject, ref pureDestinationObj, destType);
                    return pureDestinationObj;
                }
            }else if (destType.IsSubtypeOf(DestinationType))
            {
                pureDestinationObj = Map(sourceObject, DestinationType, pureDestinationObj);
                var subMap = Utils.First(_subtypeMappings, m => m.DestinationType == destType);
                return subMap.Map(sourceObject, destType, pureDestinationObj);
            }
            return default(D);
        }

        public void MapOnto<S, D>(S sourceObject, ref D destObject)
        {
            MapOnto(sourceObject, ref destObject, DestinationType);
        }

        protected void MapOnto<S, D>(S sourceObject, ref D destObject, TDestination destType)
        {
            foreach (var prop in destType.Properties)
            {
                if (_propertyMappings.TryGetValue(prop.Name, out var map))
                    map.MapOnto(sourceObject, destObject);
                else
                {
                    var identityProp = Utils.FirstOrDefault(SourceType.Properties, p => p.Name == prop.Name && p.Range == prop.Range);
                    if (identityProp != default)
                    {
                        map = Property(prop.Name).FromProperty(identityProp.Name);
                        map.MapOnto(sourceObject, destObject);
                    }
                }
            }
        }

        public List<D> Map<S, D>(ICollection<S> objs)
        {
            var list = new List<D>(objs.Count);
            foreach (S obj in objs)
                list.Add(Map<S,D>(obj));
            return list;
        }


    }

    public class MappingPoco<TSource, TDestination> : InterMapping<ObjectDescription, ObjectDescription>
    {
        public static MappingPoco<TSource, TDestination> New()
        {
            return  new();
        }

        protected MappingPoco() : base(ObjectDescription.Get<TSource>(), ObjectDescription.Get<TDestination>())
        {
        }

        public MappingPoco<TSource, TDestination> UsingCustomInstantiation(Func<TSource, TDestination> init) 
        {
            SourceType = ObjectDescription.Get<TSource>();
            base.UsingCustomInstantiation(init);
            return this;
        }

        public InterMapping<ObjectDescription, ObjectDescription> ForType<T>()
        {
            return ForType(ObjectDescription.Get<T>());
        }

        public TDestination Map(TSource source)
        {
            var pureObj = _initInstance(source);
            var destType = ObjectDescription.Get(pureObj);
            if (DestinationType == destType)
                return base.Map<TSource, TDestination>(source);
            return Map(source, destType, (TDestination)pureObj);
        }

        public List<TDestination> Map(ICollection<TSource> source)
        {
            return base.Map<TSource, TDestination>(source);
        }
    }

    public class WeakTypeMapping : InterMapping<WeakType, WeakType>
    {
        private WeakTypeSystem _weakTypeSystem;

        public static WeakTypeMapping New(WeakType sourceType, WeakType destinationType)
        {
            return new(sourceType, destinationType);
        }

        protected WeakTypeMapping(WeakType sourceType, WeakType destinationType) : base(sourceType, destinationType)
        {
            _weakTypeSystem = new WeakTypeSystem();
        }

        public WeakTypeMapping UsingCustomInstantiation(Func<WeakInstance, WeakInstance> init)
        {
            base.UsingCustomInstantiation(init);
            return this;
        }

        public override InterMapping<WeakType, WeakType> ForType(WeakType type)
        {
            return base.ForType(type);
        }

        public WeakInstance Map(WeakInstance source, WeakType destType)
        {
            var pureObj = (WeakInstance)_initInstance(source);
            if (_weakTypeSystem.IsInstanceOf(pureObj, DestinationType))
                return base.Map<WeakInstance, WeakInstance>(source);
            var subMap = Utils.FirstOrDefault(_subtypeMappings, m => _weakTypeSystem.IsInstanceOf(pureObj, m.DestinationType));
            if (subMap != default)
                return base.Map(source, subMap.DestinationType, pureObj);
            return null;
        }

        public WeakInstance Map(WeakInstance source)
        {
            return Map(source, DestinationType);
        }

        public List<WeakInstance> Map(ICollection<WeakInstance> source)
        {
            return base.Map<WeakInstance, WeakInstance>(source);
        }

        public InterMapping<WeakType, WeakType> CustomMapping(Func<WeakInstance, WeakInstance> map)
        {
            return CustomMapping<WeakInstance, WeakInstance>(map);
        }
    }
}
