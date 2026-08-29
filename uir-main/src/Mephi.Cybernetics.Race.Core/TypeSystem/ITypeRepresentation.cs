using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace Mephi.Cybernetics.Race.Core.TypeSystem
{

    public interface ITypeRepresentation : IDomain, ISubtypingInfo<ITypeRepresentation>, IDomainSubtypingInfo, IDomainAttributesInfo
    {

    }

    public class TypeRepresentation : ITypeRepresentation
    {
        private object _sourcePoco;

        public static ClassBasedTypeRepresentationConstructor<TPoco> Define<TPoco>(TPoco o)
        {
            return new ClassBasedTypeRepresentationConstructor<TPoco>(o);
        }

        public string Name { get; init; }

        public string Namespace { get; init; }

        public string Source { get; init; }

        public string FullName { get; init; }

        public string FullyQualifiedName { get; init; }

        public bool IsSubtypeOf(ITypeRepresentation typeRepresentation)
        {
            return _isSubtypeOf(this, typeRepresentation);
        }

        private Func<ITypeRepresentation, ITypeRepresentation, bool> _isSubtypeOf;

        internal TypeRepresentation() { }

        internal static TypeRepresentation Create(string name, string @namespace, string source, IReadOnlyList<ITypeRepresentation> parents, List<ClassBasedTypeRepresentationAttributeCollectionConstructionInfo> properties, Func<ITypeRepresentation, ITypeRepresentation, bool> isSubtypeOf)
        {
            var type = new TypeRepresentation()
            {
                Name = name,
                Namespace = @namespace,
                Source = source,
                FullName = $"[{@namespace}{(!string.IsNullOrEmpty(@namespace) ? "." : "")}{name}]",
                FullyQualifiedName = $"[{source}::[{@namespace}{(!string.IsNullOrEmpty(@namespace) ? "." : "")}{name}]]",
                Parents = parents, //new ReadOnlyCollection<ITypeRepresentation>(new List<ITypeRepresentation>());
                _isSubtypeOf = isSubtypeOf
            };
            type.Properties = new TypePropertiesCollection(type, properties);
            return type;
        }

        public IReadOnlyList<ITypeRepresentation> Parents { get; init; }


        public IAttributeCollection<IDomain, IDomainAttribute> Properties { get; private set; }

        IReadOnlyList<IDomain> ISubtypingInfo<IDomain>.Parents => throw new NotImplementedException();

        public bool IsSubtypeOf(IDomain domain)
        {
            if (domain is TypeRepresentation tr && false)
            {
                return this.IsSubtypeOf(tr);
            }else if (domain is IDomainAttributesInfo dai)
            {
                return this.Properties.All(ap => dai.Properties.Any(bp => bp.Name == ap.Name && bp.Range == ap.Range));

            }
            throw new NotImplementedException();
        }
    }

    public class ClassBasedTypeRepresentationConstructor<TPoco> : ITypeRepresentationConstructor
    {
        private ClassBasedTypeRepresentationConstuctionInfo _type;
        private ClassBasedTypeRepresentationAttributeCollectionConstructor<TPoco> _attributes;
        private Func<ITypeRepresentation, ITypeRepresentation, bool> _isSubtypeOf;
        private TPoco _sourceObject;

        private static bool DefaultIsSubtypeOf(ITypeRepresentation a, ITypeRepresentation b)
        {
            return a.Properties.All(ap => b.Properties.Any(bp => bp.Name == ap.Name && bp.Range == ap.Range));
        }

        internal ClassBasedTypeRepresentationConstructor(TPoco o)
        {
            _sourceObject = o;
            _type = new ClassBasedTypeRepresentationConstuctionInfo();
        }

        public ClassBasedTypeRepresentationConstructor<TPoco> Name(string name)
        {
            _type.Name = name;
            return this;
        }

        public ClassBasedTypeRepresentationConstructor<TPoco> Name(Func<TPoco, string> selector)
        {
            _type.Name = selector(_sourceObject);
            return this;
        }

        public ClassBasedTypeRepresentationConstructor<TPoco> Namespace(string ns)
        {
            _type.Namespace = ns;
            return this;
        }

        public ClassBasedTypeRepresentationConstructor<TPoco> Namespace(Func<TPoco, string> selector)
        {
            _type.Namespace = selector(_sourceObject);
            return this;
        }

        public ClassBasedTypeRepresentationConstructor<TPoco> Source(string source)
        {
            _type.Source = source;
            return this;
        }

        public ClassBasedTypeRepresentationConstructor<TPoco> Source(Func<TPoco, string> selector)
        {
            _type.Source = selector(_sourceObject);
            return this;
        }

        public ClassBasedTypeRepresentationConstructor<TPoco> IsSubtypeOf(Func<ITypeRepresentation, ITypeRepresentation, bool> isSubtypeOf)
        {
            _isSubtypeOf = isSubtypeOf;
            return this;
        }


        public ClassBasedTypeRepresentationAttributeCollectionConstructor<TPoco> Attributes()
        {
            return _attributes = new ClassBasedTypeRepresentationAttributeCollectionConstructor<TPoco>(this, _sourceObject);
        }



        public TypeRepresentation End()
        {
            if (string.IsNullOrEmpty(_type.Name))
                throw new ArgumentException("Required parameter 'Name' not set");
            var type = TypeRepresentation.Create(_type.Name, _type.Namespace, _type.Source, new ReadOnlyCollection<ITypeRepresentation>(new List<ITypeRepresentation>()), _attributes.GetAttributesInfos(), _isSubtypeOf ?? DefaultIsSubtypeOf);
            return type;
        }
    }

    internal struct ClassBasedTypeRepresentationConstuctionInfo
    {
        public string Name;
        public string Namespace;
        public string Source;
    }

    //public class ClassBasedTypeRepresentationConstructor<TPoco> : ITypeRepresentationConstructor
    //{
    //    private ClassBasedTypeRepresentationConstuctionInfo _type;
    //    private ClassBasedTypeRepresentationAttributeCollectionConstructor _attributes;
    //    private Func<ITypeRepresentation, ITypeRepresentation, bool> _isSubtypeOf;
    //    private object _sourceObject;


    //    private static bool DefaultIsSubtypeOf(ITypeRepresentation a, ITypeRepresentation b)
    //    {
    //        return a.Properties.All(ap => b.Properties.Any(bp => bp.Name == ap.Name && bp.Range == ap.Range));
    //    }

    //    internal ClassBasedTypeRepresentationConstructor(object o)
    //    {
    //        _sourceObject = o;
    //        _type = new ClassBasedTypeRepresentationConstuctionInfo();
    //    }

    //    public ClassBasedTypeRepresentationConstructor Name(string name)
    //    {
    //        _type.Name = name;
    //        return this;
    //    }

    //    public ClassBasedTypeRepresentationConstructor Namespace(string ns)
    //    {
    //        _type.Namespace = ns;
    //        return this;
    //    }

    //    public ClassBasedTypeRepresentationConstructor Source(string source)
    //    {
    //        _type.Source = source;
    //        return this;
    //    }

    //    public ClassBasedTypeRepresentationConstructor IsSubtypeOf(Func<ITypeRepresentation, ITypeRepresentation, bool> isSubtypeOf)
    //    {
    //        _isSubtypeOf = isSubtypeOf;
    //        return this;
    //    }


    //    public ClassBasedTypeRepresentationAttributeCollectionConstructor<TPoco> Attributes()
    //    {
    //        return _attributes = new ClassBasedTypeRepresentationAttributeCollectionConstructor<TPoco>(this);
    //    }



    //    public TypeRepresentation End()
    //    {
    //        if (string.IsNullOrEmpty(_type.Name))
    //            throw new ArgumentException("Required parameter 'Name' not set");
    //        var type = TypeRepresentation.Create(_type.Name, _type.Namespace, _type.Source, new ReadOnlyCollection<ITypeRepresentation>(new List<ITypeRepresentation>()), _attributes.GetAttributesInfos(), _isSubtypeOf ?? DefaultIsSubtypeOf);
    //        return type;
    //    }


    //}

    internal struct ClassBasedTypeRepresentationAttributeCollectionConstructionInfo
    {
        public string Name;
        public IDomain Range;
        public bool IsComputable;
    }

    public class ClassBasedTypeRepresentationAttributeCollectionConstructor<TPoco>
    {
        

        private List<ClassBasedTypeRepresentationAttributeCollectionConstructionInfo> _attributes;
        private readonly ClassBasedTypeRepresentationConstructor<TPoco> _typeConstuctor;
        private readonly TPoco _sourceObj;

        internal ClassBasedTypeRepresentationAttributeCollectionConstructor(ClassBasedTypeRepresentationConstructor<TPoco> typeConstuctor, TPoco sourceObj)
        {
            _attributes = new List<ClassBasedTypeRepresentationAttributeCollectionConstructionInfo>();
            this._typeConstuctor = typeConstuctor;
            _sourceObj = sourceObj;
        }

        public ClassBasedTypeRepresentationAttributeCollectionConstructor<TPoco> Add(string name, IDomain range, bool isComputable = false)
        {
            _attributes.Add(new ClassBasedTypeRepresentationAttributeCollectionConstructionInfo { Name = name, Range = range, IsComputable = isComputable });
            return this;
        }

        public ClassBasedTypeRepresentationAttributeCollectionConstructor<TPoco> AddFromCollection<TItem>(Func<TPoco, IEnumerable<TItem>> selector, Func<TItem, (string name, IDomain range)> map)
        {
            var list = selector(_sourceObj);
            foreach(var e in list)
            {
                var (n, r) = map(e);
                _attributes.Add(new ClassBasedTypeRepresentationAttributeCollectionConstructionInfo { Name = n, Range = r });
            }
            return this;
        }

        public ClassBasedTypeRepresentationAttributeCollectionConstructor<TPoco> CopyFromProperty(Func<TPoco, string> selector)
        {
            var attrName = selector(_sourceObj);
            var attr = ObjectDescription.Get(_sourceObj).Properties[attrName];
            _attributes.Add(new ClassBasedTypeRepresentationAttributeCollectionConstructionInfo { Name = attr.Name, Range = attr.Range });
            return this;
        }

        public ClassBasedTypeRepresentationConstructor<TPoco> End()
        {
            return _typeConstuctor;
        }

        internal List<ClassBasedTypeRepresentationAttributeCollectionConstructionInfo> GetAttributesInfos()
        {
            return _attributes;
        }
    }


    public class TypePropertiesCollection : IDomainAttributeCollection
    {
        private readonly TypeRepresentation _owner;
        private readonly Dictionary<string, TypePropertyDescription> _attributes;
        private readonly List<TypePropertyDescription> _attributesAsList;

        internal TypePropertiesCollection(TypeRepresentation type,
            IList<ClassBasedTypeRepresentationAttributeCollectionConstructionInfo> attributes)
        {
            _owner = type;
            _attributes = attributes
                .Select(a => new TypePropertyDescription(a.Name, type, a.Range))
                .ToDictionary(a => a.Name, a => a);
            _attributesAsList = _attributes.Values.ToList();
        }

        public TypePropertyDescription this[int index] => _attributesAsList[index];

        public TypePropertyDescription this[string name] => _attributes[name];

        IDomainAttribute IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.this[int index] => _attributesAsList[index];

        IDomainAttribute IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.this[string name] => _attributes[name];

        public int Count => _attributes.Count;

        public TypeRepresentation ReflectedDomain => _owner;

        IDomain IAttributeCollection2<IDomain, IDomain, IDomainAttribute>.ReflectedDomain => _owner;

        public bool Contains(string name)
        {
            return _attributes.ContainsKey(name);
        }

        public IEnumerator<TypePropertyDescription> GetEnumerator()
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

    public class TypePropertyDescription : IAttributeBase<TypeRepresentation>, IDomainAttribute
    {

        public string Name { get; init; }

        public TypeRepresentation Domain { get; init; }

        public IDomain Range { get; init; }

        public bool IsReadOnly { get; init; }

        IDomain IAttributeBase2<IDomain, IDomain>.Domain { get; }

        IDomain IAttributeBase2<IDomain, IDomain>.Range { get; }

        TypeRepresentation IAttributeBase2<TypeRepresentation, TypeRepresentation>.Range => Domain;

        public object GetValue(object o)
        {
            if(o is WeakInstance instance)
            {
                return instance[Name];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public T GetValue<T>(object o)
        {
            if (o is WeakInstance instance)
            {
                return (T)instance[Name];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SetValue(object o, object value)
        {
            if (o is WeakInstance instance)
            {
                instance[Name] = value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SetValue<T>(object o, T value)
        {
            if (o is WeakInstance instance)
            {
                instance[Name] = value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public TypePropertyDescription(string name, TypeRepresentation domain, IDomain range)
        {
            this.Name = name;
            this.Range = range;
            this.Domain = domain;
        }
    }
}
