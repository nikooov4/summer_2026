using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.TypeSystem;


namespace Mephi.Cybernetics.Race.Core.TypeSystem
{

    public class WeakTypeSystem : ITypeSystem
    {
        private HashSet<WeakType> _weakTypes;

        public WeakTypeSystem()
        {
            _weakTypes = new();
        }

        public WeakTypeSystem(IEnumerable<WeakType> types)
        {
            _weakTypes = new(types);
        }

        public bool IsSubtypingSupported => true;

        public bool IsAttributesSupported => true;

        public ICollectionDomain GetCollectionInfo(IDomain domain)
        {
            throw new NotImplementedException();
        }

        public IDomainAttributeCollection GetDomainAttributes(IDomain domain)
        {
            return new WeakTypeAttributeCollection(null, null);
        }

        public IFunctionTypeInfo GetFunctionTypeInfo(IDomain domain)
        {
            return null;
        }

        public IDomainStructureInfo GetStructureInfo(IDomain domain)
        {
            return null;
        }

        public IDomainSubtypingInfo GetSubtypingInfo(IDomain domain)
        {
            if (domain is WeakType weakType)
                return weakType;
            throw new NotImplementedException();
        }

        public bool IsCollection(IDomain domain)
        {
            return false;
        }

        public bool IsComposite(IDomain domain)
        {
            return false;
        }

        public bool IsFunction(IDomain domain)
        {
            return false;
        }

        public bool IsInstanceOf(object obj, IDomain domain)
        {
            if (domain is ObjectDescription od && od.IsAssumedPrimitive)
                return true;
            if (obj is WeakInstance weakInstance)
            {
                if (domain is WeakType weakType)
                    return weakType.Properties.Count == weakInstance.Properties.Count && weakType.Properties.All(p =>
                        weakInstance.HasAttibute(p.Name) && IsInstanceOf(weakInstance[p.Name], p.Range));
                else if (domain is ObjectDescription obDesc)
                {
                    return ((IEnumerable<PropertyDescription>)obDesc.Properties).All(p =>
                        weakInstance.HasAttibute(p.Name) && IsInstanceOf(weakInstance[p.Name], p.Range));
                }
            }
               
            throw new NotImplementedException();
        }

        public bool IsSubtypeOf(IDomain domain1, IDomain domain2)
        {
            if (domain1 is WeakType weakType1 && domain2 is WeakType weakType2)
                return weakType1.IsSubtypeOf(weakType2);
            if (domain1 is IDomainAttributesInfo attr1 && domain2 is IDomainAttributesInfo attr2)
//          Почему-то не работает, хз 
//            return attr2.Properties.All(a => attr1.Properties.Contains(a.Name) &&
//                a.Range is IDomainSubtypingInfo poDomain
//                    ? poDomain.IsSubtypeOf(attr1.Properties[a.Name].Range)
//                    : a.Range == attr1.Properties[a.Name].Range);
                return attr2.Properties.All(
                    a => attr1.Properties.Contains(a.Name)
                        ? a.Range is IDomainSubtypingInfo poDomain
                            ? poDomain.IsSubtypeOf(attr1.Properties[a.Name].Range)
                            : a.Range == attr1.Properties[a.Name].Range
                        : false);
            return false;
        }

        public void AddType(WeakType type)
        {
            _weakTypes.Add(type);
        }
    }

    public class WeakInstance : IEquatable<WeakInstance>
    {
        private Dictionary<string, object> _values;
        private bool _canEdit;
        private bool _isSilent;

        public WeakInstance(Dictionary<string, object> values)
        {
            _values = new Dictionary<string, object>(values);
            _canEdit = true;
            _isSilent = false;
        }

        public WeakInstance()
        {
            _values = new Dictionary<string, object>();
            _canEdit = true;
            _isSilent = false;
        }

        public object this[string name]
        {
            get { return _values[name]; }
            set
            {
                if (_canEdit)
                    _values[name] = value;
                else if (!_isSilent)
                    throw new InvalidOperationException("Cannot alter an immutable entity");
            }
        }

        public IDictionary<string, object> Properties => _values;

        public bool Equals(WeakInstance other)
        {
            if (_canEdit != other._canEdit || _isSilent != other._isSilent || _values.Count != other._values.Count)
                return false;
            foreach (var pair in _values)
            {
                if (!other._values.TryGetValue(pair.Key, out var otherValObj) || otherValObj == null && pair.Value != null)
                    return false;
                if (otherValObj == null && pair.Value == null || otherValObj.Equals(pair.Value))
                    continue;
                var otherVal = otherValObj as WeakInstance;
                if (otherVal == null || !(otherVal.Equals(pair.Value)))
                    return false;
            }
            return true;
        }

        public bool HasAttibute(string name)
        {
            return _values.ContainsKey(name);
        }

        public Optional<object> TryGet(string name)
        {
            if (!_values.ContainsKey(name))
                return Optional<object>.None;
            return _values[name];
        }
    }

    public class WeakType : IDomain, ISubtypingInfo<WeakType>, IDomainAttributesInfo, IDomainSubtypingInfo
    {
        public static WeakType From(object o)
        {
            return From(ObjectDescription.Get(o));
        }

        public static WeakType From<T>()
        {
            return From(typeof(T));
        }

        public static WeakType From(Type type)
        {
            return From(ObjectDescription.Get(type));
        }

        public static WeakType From(ObjectDescription od)
        {
            
            var typeConstuctor = Define()
                .Name(od.Name)
                .Namespace(od.Namespace)
                .Source(od.Source)
                .Attributes();

            foreach(var p in od.Properties)
            {
                IDomain domain = p.Range;
                if (!p.Range.IsAssumedPrimitive)
                    domain = From(p.Range);
                typeConstuctor.Add(p.Name, domain, p.IsComputable);
            }
            var res = typeConstuctor.End().End();
            return res;
        }

        public static WeakTypeConstuctor Define()
        {
            return new WeakTypeConstuctor();
        }

        private ReadOnlyCollection<WeakType> _parents;

        internal WeakType(string name, string ns, string source,
            IList<WeakTypeAttributeCollectionConstructor.WeakTypeAttributeConstructionInfo> attributes)
        {
            Name = name;
            Namespace = ns;
            Source = source;
            Properties = new WeakTypeAttributeCollection(this, attributes);
            _parents = new ReadOnlyCollection<WeakType>(new List<WeakType>());
        }

        public string Name { get; }

        public string Namespace { get; }

        public string Source { get; }

        public string FullName => $"[{Namespace}{(!string.IsNullOrEmpty(Namespace) ? "." : "")}{Name}]";

        public string FullyQualifiedName => $"[{Source}::{FullName}]";

        public WeakTypeAttributeCollection Properties { get; }

        IAttributeCollection<IDomain, IDomainAttribute> IDomainAttributesInfo.Properties => Properties;

        public IReadOnlyList<WeakType> Parents => _parents;

        IReadOnlyList<IDomain> ISubtypingInfo<IDomain>.Parents => _parents;

        public bool IsSubtypeOf(WeakType domain)
        {
            bool domainIsSubtypeOfOrEqual(IDomain dom1, IDomain dom2)
            {
                if (dom1 is IDomainSubtypingInfo poDomain)
                    return poDomain.IsSubtypeOf(dom2);
                return dom1 == dom2;
            }

            return domain.Properties.All(
                a => Properties.Contains(a.Name)
                    ? domainIsSubtypeOfOrEqual(Properties[a.Name].Range, a.Range)
                    : false);
        }

        public bool IsSubtypeOf(IDomain domain)
        {
            if (domain is WeakType weakType)
                return IsSubtypeOf(weakType);
            throw new NotImplementedException();
        }
    }

    public class WeakTypeAttribute : IAttributeBase<WeakType>, IDomainAttribute // типизация в C# какашка ?..
    {
        public WeakTypeAttribute(string name, WeakType domain, bool isComputable, IDomain range)
        {
            Name = name;
            Domain = domain;
            Range = range;
            _isComputable = isComputable;
        }

        public string Name { get; }

        public WeakType Domain { get; }

        IDomain IAttributeBase2<IDomain, IDomain>.Domain => Domain;

        public IDomain Range { get; }
        WeakType IAttributeBase2<WeakType, WeakType>.Range { get; }

        public bool IsReadOnly => false;

        private bool _isComputable;

        public bool IsComputable => _isComputable;


        public object GetValue(WeakInstance instance)
        {
            return instance[Name];
        }

        public T GetValue<T>(WeakInstance instance)
        {
            return (T)instance[Name];
        }

        public void SetValue(WeakInstance instance, object value)
        {
            instance[Name] = value;
        }

        public void SetValue<T>(WeakInstance instance, T value)
        {
            instance[Name] = value;
        }

        object IAttributeBase2<WeakType, WeakType>.GetValue(object o)
        {
            if (o is WeakInstance instance)
                return GetValue(instance);
            throw new ArgumentException("Cannot get property value of an object that is not a weak instance");
        }

        T IAttributeBase2<WeakType, WeakType>.GetValue<T>(object o)
        {
            if (o is WeakInstance instance)
                return GetValue<T>(instance);
            throw new ArgumentException("Cannot get property value of an object that is not a weak instance");
        }

        void IAttributeBase2<WeakType, WeakType>.SetValue(object o, object value)
        {
            if (o is WeakInstance instance)
            {
                SetValue(instance, (WeakInstance)value);
                return;
            }
            throw new ArgumentException("Cannot set property value of an object that is not a weak instance");
        }

        void IAttributeBase2<WeakType, WeakType>.SetValue<T>(object o, T value)
        {
            if (o is WeakInstance instance)
            {
                SetValue(instance, value);
                return;
            }
            throw new ArgumentException("Cannot set property value of an object that is not a weak instance");
        }

        object IAttributeBase2<IDomain, IDomain>.GetValue(object o)
        {
            if (o is WeakInstance instance)
                return GetValue(instance);
            throw new ArgumentException("Cannot get property value of an object that is not a weak instance");
        }

        T IAttributeBase2<IDomain, IDomain>.GetValue<T>(object o)
        {
            if (o is WeakInstance instance)
                return GetValue<T>(instance);
            throw new ArgumentException("Cannot get property value of an object that is not a weak instance");
        }

        void IAttributeBase2<IDomain, IDomain>.SetValue(object o, object value)
        {
            if (o is WeakInstance instance)
            {
                SetValue(instance, value);
                return;
            }
            throw new ArgumentException("Cannot set property value of an object that is not a weak instance");
        }

        void IAttributeBase2<IDomain, IDomain>.SetValue<T>(object o, T value)
        {
            if (o is WeakInstance instance)
            {
                SetValue(instance, value);
                return;
            }
            throw new ArgumentException("Cannot set property value of an object that is not a weak instance");
        }

    }

    public class
        WeakTypeAttributeCollection : IDomainAttributeCollection //, IAttributeCollection<WeakType, WeakTypeAttribute> // не работает из-за разновидности diamond-наследования ???
    {
        private readonly WeakType _owner;
        private readonly Dictionary<string, WeakTypeAttribute> _attributes;
        private readonly List<WeakTypeAttribute> _attributesAsList;

        internal WeakTypeAttributeCollection(WeakType weakType,
            IList<WeakTypeAttributeCollectionConstructor.WeakTypeAttributeConstructionInfo> attributes)
        {
            _owner = weakType;
            _attributes = attributes
                .Select(a => new WeakTypeAttribute(a.Name, weakType, a.IsComputable, a.Range))
                .ToDictionary(a => a.Name, a => a);
            _attributesAsList = _attributes.Values.ToList();
        }

        public WeakTypeAttribute this[int index] => _attributesAsList[index];

        public WeakTypeAttribute this[string name] => _attributes[name];

        IDomainAttribute IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.this[int index] => _attributesAsList[index];

        IDomainAttribute IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.this[string name] => _attributes[name];

        public int Count => _attributes.Count;

        public WeakType ReflectedDomain => _owner;

        IDomain IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.ReflectedDomain => _owner;

        public bool Contains(string name)
        {
            return _attributes.ContainsKey(name);
        }

        public IEnumerator<WeakTypeAttribute> GetEnumerator()
        {
            return _attributesAsList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _attributesAsList.GetEnumerator();
        }

        IEnumerator<IDomainAttribute> IEnumerable<IDomainAttribute>.GetEnumerator()
        {
            return _attributesAsList.GetEnumerator();
        }
    }

    public class WeakTypeConstuctor
    {
        internal struct WeakTypeConstuctionInfo
        {
            public string Name;
            public string Namespace;
            public string Source;
        }

        private WeakTypeConstuctionInfo _type;
        private WeakTypeAttributeCollectionConstructor _attributes;

        internal WeakTypeConstuctor()
        {
            _type = new WeakTypeConstuctionInfo();
        }

        public WeakTypeConstuctor Name(string name)
        {
            _type.Name = name;
            return this;
        }

        public WeakTypeConstuctor Namespace(string ns)
        {
            _type.Namespace = ns;
            return this;
        }

        public WeakTypeConstuctor Source(string source)
        {
            _type.Source = source;
            return this;
        }

        public WeakTypeAttributeCollectionConstructor Attributes()
        {
            return _attributes = new WeakTypeAttributeCollectionConstructor(this);
        }

        public WeakType End()
        {
            if (string.IsNullOrEmpty(_type.Name))
                throw new ArgumentException("Required parameter 'Name' not set");
            var type = new WeakType(_type.Name, _type.Namespace, _type.Source, _attributes.GetAttributesInfos());
            return type;
        }
    }

    public class WeakTypeAttributeCollectionConstructor
    {
        internal struct WeakTypeAttributeConstructionInfo
        {
            public string Name;
            public IDomain Range;
            public bool IsComputable;
        }

        private List<WeakTypeAttributeConstructionInfo> _attributes;
        private readonly WeakTypeConstuctor _weakTypeConstuctor;

        internal WeakTypeAttributeCollectionConstructor(WeakTypeConstuctor weakTypeConstuctor)
        {
            _attributes = new List<WeakTypeAttributeConstructionInfo>();
            this._weakTypeConstuctor = weakTypeConstuctor;
        }

        public WeakTypeAttributeCollectionConstructor Add(string name, IDomain range, bool isComputable=false)
        {
            _attributes.Add(new WeakTypeAttributeConstructionInfo { Name = name, Range = range, IsComputable=isComputable });
            return this;
        }

        public WeakTypeConstuctor End()
        {
            return _weakTypeConstuctor;
        }

        internal List<WeakTypeAttributeConstructionInfo> GetAttributesInfos()
        {
            return _attributes;
        }
    }
}
