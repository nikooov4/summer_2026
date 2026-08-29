using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Mephi.Cybernetics.Race.Core.Attributes.ComplexObjects;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling;
using ITypeSystem = Mephi.Cybernetics.Race.Core.TypeSystem.ITypeSystem;
using Math = System.Math;

//using Mephi.Cybernetics.Race.HostEnvironments.TypeSystem;

namespace Mephi.Cybernetics.Race.Core
{
    //TODO: Add the support for generics
    public class ObjectDescription : IDomain, IDomainSubtypingInfo, IDomainAttributesInfo
    {
//        private static ObjectDescriptionFactory _odFactory;
//        private static readonly ObjectDescription SystemAttributeDescription;
//        private static readonly Dictionary<Type, ObjectDescription> _metaObjects = new Dictionary<Type, ObjectDescription>();
//        protected static List<Type> primitiveTypes;
//        protected static ObjectDescription[] primitiveTypesDescriptions;
//        private static readonly ObjectDescription _delegateDescription;
        private static readonly ObjectDescriptionConstructor DefaultObjectDescriptionConstructor;
        private static readonly IStrictTypeSystem TypeSystemDescription_;
//
//        public ObjectDescription[] PrimitiveTypesDescriptions
//        {
//            get { return primitiveTypesDescriptions; }
//        }
//

        private class TypeSystemInfo : IStrictTypeSystem
        {
            public IDomain GetTypeInfo(object obj)
            {
                return ObjectDescription.Get(obj);
            }

            public bool IsInstanceOf(object obj, IDomain domain)
            {
                if (domain is ObjectDescription od)
                {
                    var objOd = Get(obj.GetType());
                    return objOd.IsSubtypeOf(od);
                }
                else
                    throw new ArgumentException("Unrecognized type");
            }

            public bool IsSubtypingSupported => true;

            public bool IsSubtypeOf(IDomain domain1, IDomain domain2)
            {
                if (domain1 is ObjectDescription od1 && domain2 is ObjectDescription od2)
                    return od1.IsSubtypeOf(od2);
                else
                    throw new ArgumentException("Unrecognized types");
            }

            public IDomainSubtypingInfo GetSubtypingInfo(IDomain domain)
            {
                if (domain is ObjectDescription od)
                    return od;
                else
                    throw new ArgumentException("Unrecognized type");
            }

            public bool IsAttributesSupported => true;

            public IDomainAttributeCollection GetDomainAttributes(IDomain domain)
            {
                if (domain is ObjectDescription od)
                    return od.Properties;
                else
                    throw new ArgumentException("Unrecognized type");
            }

            public bool IsFunction(IDomain domain)
            {
                throw new NotImplementedException();
                if (domain is ObjectDescription od)
                    return true;
                else
                    throw new ArgumentException("Unrecognized type");
            }

            public IFunctionTypeInfo GetFunctionTypeInfo(IDomain domain)
            {
                throw new NotImplementedException();
            }

            public bool IsComposite(IDomain domain)
            {
                throw new NotImplementedException();
            }

            public IDomainStructureInfo GetStructureInfo(IDomain domain)
            {
                throw new NotImplementedException();
            }

            public bool IsCollection(IDomain domain)
            {
                throw new NotImplementedException();
            }

            public ICollectionDomain GetCollectionInfo(IDomain domain)
            {
                throw new NotImplementedException();
            }
        }

        public static IStrictTypeSystem TypeSystemDescription => TypeSystemDescription_;

        public static ObjectDescription DelegateDescription
        {
            get { return DefaultObjectDescriptionConstructor.DelegateOD; }
        }

        static ObjectDescription()
        {
            ObjectDescription.DefaultObjectDescriptionConstructor = new ObjectDescriptionConstructor();
            ObjectDescription.TypeSystemDescription_ = new TypeSystemInfo();
        }


//        public static void SetObjectFactory(Type type, DynamicEntityBuilder dynamicEntityBuilder)
//        {
//            throw new NotImplementedException();
//        }


        public static ObjectDescription Get(Type type)
        {
            return ObjectDescription.DefaultObjectDescriptionConstructor.GetObjectDescription(type);
        }

        public static ObjectDescription Get<T>()
        {
            return ObjectDescription.DefaultObjectDescriptionConstructor.GetObjectDescription<T>();
        }

        public static ObjectDescription Get(object o)
        {
            return ObjectDescription.DefaultObjectDescriptionConstructor.GetObjectDescription(o.GetType());
        }


        private Type _type = null;
        private ObjectFactoryDescription _factoryDescription = null;
        private AttributeContainer _clrAttributeContainer = null;
        private ObjectPropertiesCollection _objectProperties = null;
        private ObjectPropertiesCollection _staticProperties = null;
        /// <summary>
        /// Только для Explicit Interface Property Implmentations (которые компилятор помечает как приватные)
        /// </summary>
        private ObjectPropertiesCollection _privateProperties = null;
        private ObjectDescription[] _parents = null;
        private ObjectDescription _baseClass = null;
        private ObjectDescription[] _allParentsChacheList = null;
        private bool _canCreateInstance = true;
        private InterfaceMemberMappingImpl _memberMapping = null;
        private int _arity = 0;
        private bool _isAtomic = true;
        private bool _isComplex = false;
        private bool _isFunction = false;
        private FunctionDescription _functionDescription = null;
        private ComplexObjectDescription _complexObjectDescription = null;
        private bool _isAssumedPrimitive;

        protected internal ObjectDescription()
        {
        }

        public void FillPropertyCollections()
        {
            var staticPropertyIndexCounter =  0;
            var propertyIndexCounter = _baseClass?.Properties.Count ?? 0;

            int GetPropertyIndex(PropertyInfo prop)
            {
                if (_baseClass != null && _baseClass.Properties.Contains(prop.Name))
                    return _baseClass.Properties[prop.Name].Index;
                else
                    return propertyIndexCounter++;
            }

            void AddProperty(PropertyInfo prop)
            {
                if (prop.GetGetMethod() == null || prop.Name == "Item" /*|| prop.GetCustomAttributes(typeof(SubObjectAttribute), true).Length > 0*/)
                    // what is a property with no getter? Nonsense.
                    return;

                if (!_objectProperties.Contains(prop.Name))
                {
                    var pd = new PropertyDescription(this, prop, GetPropertyIndex(prop));
                    _objectProperties.AddProperty(pd);
                }
                else
                {
                    var oldPd = _objectProperties[prop.Name];
                    var pd = new PropertyDescription(this, prop, oldPd.Index);
                    _objectProperties.Replace(pd);
                }
            }

            
            foreach (var staticProperty in _type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (staticProperty.GetGetMethod() == null) 
                    // what is a property with no getter? Nonsense.
                    continue;
                if (staticProperty.GetGetMethod().IsAbstract)
                    // requires special treatment?
                    //continue;
                    ;
                var spd = new PropertyDescription(this, staticProperty, staticPropertyIndexCounter++);
                _staticProperties.AddProperty(spd);
            }
            foreach ( var prop in _type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) )
                AddProperty(prop);
            if (_type.IsInterface)
                foreach (var parent in Parents)
                    foreach ( var property in parent.Properties )
                        AddProperty(property.Property);

        }


        public ObjectDescription[] Parents => _parents;

        IReadOnlyList<IDomain> ISubtypingInfo<IDomain>.Parents => _parents;

        public Type ClrType => _type;

        //TODO: вернуть internal
        public AttributeContainer ClrAttributes => _clrAttributeContainer;


        public ObjectFactoryDescription FactoryDescription => _factoryDescription;

        public bool CanCreateInstance => _canCreateInstance;

        public ObjectPropertiesCollection Properties => _objectProperties;
        public ObjectPropertiesCollection StaticProperties => _staticProperties;
        IAttributeCollection<IDomain, IDomainAttribute> IDomainAttributesInfo.Properties => _objectProperties;

        public int Arity => _arity;

        public bool IsAtomic => _isAtomic;

        public bool IsComplex => _isComplex;

        public bool IsFunction => _isFunction;

        //TODO: вернуть internal
        public InterfaceMemberMapping InterfaceMapping => _memberMapping;

        public string Name => _type.Name;

        public string FullName => _type.FullName;

        public string Namespace => _type.Namespace;

        public string Source => _type.Assembly.FullName;

        public string FullyQualifiedName => $"[{Source}::{FullName}]";

        public bool IsAssumedPrimitive => _isAssumedPrimitive;

        private ObjectDescription[] GetAllParentsCacheList()
        {
            var parentClasses = new List<ObjectDescription>();
            var parentInterfaces = new List<ObjectDescription>();

            var tempList = new Queue<ObjectDescription>();
            foreach ( var parent in Parents )
                tempList.Enqueue(parent);
            while ( tempList.Count > 0 )
            {
                var top = tempList.Dequeue();
                if ( top.ClrType.IsInterface )
                    parentInterfaces.Add(top);
                else
                    parentClasses.Add(top);
                foreach ( var parent in top.Parents )
                    tempList.Enqueue(parent);
            }
            parentClasses.AddRange(parentInterfaces);
            return parentClasses.ToArray();
        }

        public object CreateObject(params object[] args)
        {
            if (args.Length == 0)
                return FactoryDescription.InvokeDefaultConstruction(_type);
            else
                return FactoryDescription.InvokeConstructionWithArgs(_type, args);
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
        public Attribute GetClrAttribute(Type type)
        {
            return _clrAttributeContainer.GetAttribute(type);
        }

        public bool HasClrAttribute(Type attributeType)
        {
            return this.GetClrAttribute(attributeType) != null;
        }

        /// <summary>
        /// Возвращает атрибут (тип которого производный от указанного класса), примененный к данному типу.
        /// Если таких окажется несколько, выбирается "наиболее конкретный".
        /// "Нативная" реализация выбрасывает исключение, не пытаясь разрешать неоднозначности
        /// (В теории, эта реализация должна работать на несколько порядков быстрее, чем "нативная" Type.GetCustomAttribute,
        /// за счет кеширования)
        /// </summary>
        /// <returns></returns>
        public T GetClrAttribute<T>() where T : Attribute
        {
            return _clrAttributeContainer.GetAttribute(typeof (T)) as T;
        }

        public bool HasClrAttribute<T>() where T : Attribute
        {
            return this.GetClrAttribute<T>() != null;
        }

        /// <summary>
        /// Возвращает все атрибуты, примененные к данному типу, производные от указанного класса.
        /// В теории, эта реализация должна работать быстрее, чем "нативная" Type.GetCustomAttributes
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public IList<Attribute> GetClrAttributes(Type attributeType)
        {
            return _clrAttributeContainer.GetAttributes(attributeType);
        }

        /// <summary>
        /// Возвращает все атрибуты, примененные к данному типу, производные от указанного класса.
        /// В теории, эта реализация должна работать быстрее, чем "нативная" Type.GetCustomAttributes
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public IList<Attribute> GetClrAttributes<T>()
        {
            return _clrAttributeContainer.GetAttributes<T>();
        }

        public bool IsSubtypeOf<T>()
        {
            var od = Get<T>();
            return IsSubtypeOf(od);
        }

        public bool IsSubtypeOf(Type type)
        {
            if (this._type == type) // reflexivity
                return true;
            foreach ( var objectDescription in _allParentsChacheList )
                if ( objectDescription.ClrType == type )
                    return true;
            return false;
        }

        public bool IsSubtypeOf(ObjectDescription potentialParent)
        {
            if ( potentialParent.ClrType == _type ) // reflexivity
                return true;
            else
            {
                foreach ( var objectDescription in _allParentsChacheList )
                    if ( objectDescription.ClrType == potentialParent.ClrType )
                        return true;
                return false;
            }
        }

        bool ISubtypingInfo<IDomain>.IsSubtypeOf(IDomain domain)
        {
            // ReSharper disable once TryCastAndCheckForNull.1
            // And what if, all of a sudden, ObjectDescription happens to have inherited types?..
            if (domain as ObjectDescription != null)
                return IsSubtypeOf(domain as ObjectDescription);
            else
                return false;
        }

        private int GetDistanceFromSupertype(Type type)
        {
            if (type.IsClass && !_type.IsInterface)
                return -1;
            else if (type.IsClass && _type.IsClass)
            {
                var count = 0;
                for (var t = _type; t != null; t = t.BaseType)
                {
                    count++;
                    if (t == type)
                        return count;
                }
                return -1;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool Equals(ObjectDescription objectDescription)
        {
            return _type == objectDescription._type;
        }

        public bool Equals(Type type)
        {
            return _type == type;
        }

        public override string ToString()
        {
            return _type.ToString();
        }

        private class InterfaceMemberMappingImpl : InterfaceMemberMapping
        {
            private readonly ObjectDescription _objectDescription;
            private readonly Dictionary<MethodInfo, PropertyInfo> _getMethod2PropertyInfo;
            private readonly Dictionary<PropertyInfo, PropertyInfo> _interfacePropertyMapping;

            public InterfaceMemberMappingImpl(ObjectDescription objectDescription)
            {
                _objectDescription = objectDescription;
                if (!(_objectDescription._type.IsInterface /*|| _objectDescription._type.IsArray*/))
                {
                    var privatePropertyIndex = 0;
                    PropertyDescription.Func<PropertyInfo, PropertyDescription> createPrivateProperty = propertyInfo =>
                    {
                        var property = new PropertyDescription(_objectDescription, propertyInfo, privatePropertyIndex++);
                        _objectDescription._privateProperties.AddProperty(property);
                        return property;
                    };

                    PropertyDescription.Func<PropertyInfo, PropertyDescription> getPrivateProperty = propertyInfo =>
                    {
                        PropertyDescription.Func<ObjectDescription, PropertyInfo, PropertyDescription> getPropertyFromParents = (od, p) =>
                        {
                            while (od != null)
                            {
                                if (od._privateProperties.Contains(p))
                                    return od._privateProperties[p];
                                else
                                    od = od._baseClass;
                            }
                            throw new Exception("Property not found");
                        };
                        return propertyInfo.GetGetMethod(true).IsPrivate && propertyInfo.GetGetMethod(true).DeclaringType != _objectDescription._type
                            ? getPropertyFromParents(_objectDescription._baseClass, propertyInfo)
                            : (_objectDescription._privateProperties.Contains(propertyInfo)
                                ? _objectDescription._privateProperties[propertyInfo]
                                : createPrivateProperty(propertyInfo));
                    };

                    var interfaceMethodMapping = new Dictionary<MethodInfo, MethodInfo>();
                    foreach (var anInterface in _objectDescription._type.GetInterfaces())
                    {
                        if (_objectDescription._type.IsArray && anInterface.IsGenericType)
                        {
                            // do smth

                            // throw new NotImplementedException();
                        }
                        else
                        {
                            var interfaceMethods = _objectDescription._type.GetInterfaceMap(anInterface).InterfaceMethods;
                            var targetMethods = _objectDescription._type.GetInterfaceMap(anInterface).TargetMethods;
                            for (var index = 0; index < interfaceMethods.Length; index++)
                                interfaceMethodMapping.Add(interfaceMethods[index], targetMethods[index]);
                        }
                    }
                    _getMethod2PropertyInfo = new Dictionary<MethodInfo, PropertyInfo>();
                    var allProperties = _objectDescription._type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var propertyInfo in allProperties)
                        if ( propertyInfo.GetGetMethod(true) != null )
                            _getMethod2PropertyInfo.Add(propertyInfo.GetGetMethod(true), propertyInfo);
                    _interfacePropertyMapping = new Dictionary<PropertyInfo, PropertyInfo>();
                    foreach (var anInterface in _objectDescription._type.GetInterfaces())
                        foreach (var propertyInfo in anInterface.GetProperties())
                        {
                            if (_objectDescription._type.IsArray && anInterface.IsGenericType)
                                _interfacePropertyMapping.Add(propertyInfo, propertyInfo);
                            else
                            {
                                var targetGetMethod = interfaceMethodMapping[propertyInfo.GetGetMethod(true)];
                                if (targetGetMethod.IsPrivate && targetGetMethod.DeclaringType != _objectDescription._type)
                                    _interfacePropertyMapping.Add(propertyInfo,
                                        _objectDescription._baseClass._memberMapping._interfacePropertyMapping[propertyInfo]);
                                else
                                    _interfacePropertyMapping.Add(propertyInfo, _getMethod2PropertyInfo[targetGetMethod]);
                            }
                        }

                    foreach (var parent in _objectDescription._allParentsChacheList)
                        if (parent._type.IsInterface)
                            foreach (var property in parent.Properties)
                            {
                                var relatedPropertyInfo = _interfacePropertyMapping[property.Property];
                                AddMapping(property,
                                    relatedPropertyInfo.GetGetMethod(true).IsPublic
                                        ? (_objectDescription._type.IsArray && relatedPropertyInfo.DeclaringType.IsGenericType
                                            ? property
                                            : _objectDescription.Properties[relatedPropertyInfo])
                                        : getPrivateProperty(relatedPropertyInfo));
                            }
                }
            }

            private void AddMapping(PropertyDescription interfaceProperty, PropertyDescription classProperty)
            {
                if ( !_mapping.ContainsKey(interfaceProperty) )
                    _mapping.Add(interfaceProperty, classProperty);
            }
        }

        private class ObjectDescriptionConstructor
        {
            private readonly Dictionary<Type, ObjectDescription> _metaObjects = new Dictionary<Type, ObjectDescription>();
            private readonly IList<ObjectDescription> _primitiveTypesDescriptions;
            private readonly List<Type> _primitiveTypes;

            public ObjectDescription ObjectOD
            {
                get { return _metaObjects[typeof(object)]; }
            }

            public ObjectDescription DelegateOD
            {
                get { return _metaObjects[typeof(Delegate)]; }
            }

            public ObjectDescriptionConstructor()
            {
                _primitiveTypes =
                    new List<Type>(new Type[]
                                   {
                                       typeof(Delegate), 
                                       typeof(object), 
                                       typeof(ValueType), 
                                       //typeof(Nullable), 
                                       typeof(byte), 
                                       typeof(short), 
                                       typeof(int), 
                                       typeof(long),
                                       typeof(ushort), 
                                       typeof(uint), 
                                       typeof(ulong), 
                                       typeof(char), 
                                       typeof(DateTime), 
                                       typeof(float), 
                                       typeof(double),
                                       typeof(bool), 
                                       typeof(string), 
                                       typeof(Enum), 
                                       typeof(Attribute)
                                   });
                _primitiveTypesDescriptions = _primitiveTypes.ConvertAll(GetObjectDescription).AsReadOnly();

                var primitiveTypeConstructors =
                    new List<Type>(new Type[]
                                   {
                                       typeof(IEnumerable<>), 
                                       typeof(IList<>), 
                                       typeof(List<>), 
                                       typeof(ICollection<>)
                                       //typeof(int[]).GetGenericTypeDefinition()
                                   });
            }

            public ObjectDescription GetObjectDescription(Type type)
            {
                lock (_metaObjects)
                    if (!_metaObjects.ContainsKey(type))
                        return CreateObjectDescription(type);
                return _metaObjects[type];
            }

            public ObjectDescription GetObjectDescription<T>()
            {
                return GetObjectDescription(typeof(T));
            }

            private ObjectDescription[] GetParentTypesDescriptions(Type type)
            {
                var bases = new List<ObjectDescription>();
                if ( type.BaseType != null )
                    bases.Add(GetObjectDescription(type.BaseType));
                foreach ( var i in type.GetInterfaces() )
                {
                    var isInherited = false;
                    foreach ( var parent in bases )
                        if ( parent.IsSubtypeOf(i) )
                        {
                            isInherited = true;
                            break;
                        }
                    if ( !isInherited )
                        bases.Add(GetObjectDescription(i));
                }
                return bases.ToArray();
            }

            private ObjectDescription CreateObjectDescription(Type type)
            {
                var od = new ObjectDescription();
                od._type = type;

                _metaObjects.Add(type, od);
                
                od._parents = GetParentTypesDescriptions(type);
                od._allParentsChacheList = od.GetAllParentsCacheList();
                od._factoryDescription = new ObjectFactoryDescription(t => Activator.CreateInstance(type),
                                                               (t, args) => Activator.CreateInstance(type, args));
                od. _baseClass = od._type.IsInterface
                                    ? null
                                    : (od._type.BaseType != null
                                        ? GetObjectDescription(type.BaseType)
                                        : null);
                od._clrAttributeContainer = new AttributeContainer(od._type.GetCustomAttributes(true));
                od._isAssumedPrimitive = _primitiveTypes.Contains(type);

                if (DefaultObjectDescriptionConstructor != null)
                    od._isAssumedPrimitive = od._isAssumedPrimitive || od.IsSubtypeOf(ObjectDescription.Get<Enum>());

                od._objectProperties = new ObjectPropertiesCollection();
                od._staticProperties = new ObjectPropertiesCollection();
                od._privateProperties = new ObjectPropertiesCollection();
                od.FillPropertyCollections();
                od._memberMapping = new InterfaceMemberMappingImpl(od);
                od._objectProperties.EndInit();
                od._staticProperties.EndInit();
                od._privateProperties.EndInit();

                od._functionDescription = null; //type != typeof(Delegate) && _primitiveTypes.Contains(type) ? null : FunctionDescription.GetFunctionDescription(od);
                od._arity = od._functionDescription != null ? od._functionDescription.Arity : 0;
                od._isFunction = od._arity > 0;

                od._isComplex = od.GetClrAttribute<ObjectConstructorAttribute>() != null || od.IsSubtypeOf(typeof(IObjectConstructor));
                od._isAtomic = !od._isComplex;

                return od;
            }

            private ObjectDescription CreateOdForObject()
            {
                var type = typeof(object);
                var od = new ObjectDescription();
                od._type = type;
                od._parents = GetParentTypesDescriptions(type);
                od._factoryDescription = new ObjectFactoryDescription(t => Activator.CreateInstance(type),
                                                                        (t, args) => Activator.CreateInstance(type, args));
                od._baseClass = null;
                od._clrAttributeContainer = new AttributeContainer(od._type.GetCustomAttributes(true));
                od._objectProperties = new ObjectPropertiesCollection();
                od._privateProperties = new ObjectPropertiesCollection();
                od._functionDescription = FunctionDescription.GetFunctionDescription(od);
                od._arity = od._functionDescription != null ? od._functionDescription.Arity : 0;
                od._isFunction = od._arity > 0;

                od._isComplex = false;//GetClrAttribute<ObjectConstructorAttribute>() != null || IsSubtypeOf(typeof(IObjectConstructor));
                od._isAtomic = !od._isComplex;

                return od;
            }

            private ObjectDescription CreateOdForDelegate()
            {
                var type = typeof(Delegate);
                var od = new ObjectDescription();
                od._type = type;
                od._parents = GetParentTypesDescriptions(type);
                od._factoryDescription = new ObjectFactoryDescription(t => Activator.CreateInstance(type),
                                                                        (t, args) => Activator.CreateInstance(type, args));
                od._baseClass = GetObjectDescription<object>();
                od._clrAttributeContainer = new AttributeContainer(od._type.GetCustomAttributes(true));

                od._objectProperties = new ObjectPropertiesCollection();
                od._privateProperties = new ObjectPropertiesCollection();

                od._functionDescription = FunctionDescription.GetFunctionDescription(od);
                od._arity = od._functionDescription != null ? od._functionDescription.Arity : 0;
                od._isFunction = od._arity > 0;

                od._isComplex = false;//GetClrAttribute<ObjectConstructorAttribute>() != null || IsSubtypeOf(typeof(IObjectConstructor));
                od._isAtomic = !od._isComplex;

                return od;
            }
        }

        public IList<IEntityKey> PotentialKeys { get; }
    }

    //TODO: вернуть internal
    public abstract class InterfaceMemberMapping
    {
        protected readonly Dictionary<PropertyDescription, PropertyDescription> _mapping = new Dictionary<PropertyDescription, PropertyDescription>();

        public PropertyDescription this[PropertyDescription property]
        {
            get { return _mapping[property]; }
        }
    }

    public class CollectionDescription : ICollectionDomain
    {
        private static CollectionDescription Enumerable_;
        private static CollectionDescription EnumerableT_;
        private static CollectionDescription IListT_;
        private static CollectionDescription Array_;
        private static CollectionDescription ICollectionT_;

        public static CollectionDescription EnumerableDescription =>
            Enumerable_ ??= new CollectionDescription(ObjectDescription.Get<IEnumerable>());
        public static CollectionDescription EnumerableTDescription =>
            EnumerableT_ ??= new CollectionDescription(ObjectDescription.Get(typeof(IEnumerable<>)));
        public static CollectionDescription IListTDescription =>
            IListT_ ??= new CollectionDescription(ObjectDescription.Get(typeof(IList<>)));
        public static CollectionDescription ArrayDescription =>
            Array_ ??= new CollectionDescription(ObjectDescription.Get<Array>());
        public static CollectionDescription ICollectionTDescription =>
            ICollectionT_ ??= new CollectionDescription(ObjectDescription.Get(typeof(ICollection<>)));

        public static bool IsCollection<T>()
        {
            var od = ObjectDescription.Get<T>();
            return IsCollection(od);
        }

        public static bool IsCollection(Type type)
        {
            var od = ObjectDescription.Get(type);
            return IsCollection(od);
        }

        public static bool IsCollection(ObjectDescription od) 
        {
            throw new NotImplementedException();
        }

        public static T[] CreateArray<T>(IList<T> list)
        {
            var result = new T[list.Count];
            for (var i = 0; i < list.Count; i++)
                result[i] = list[i];
            return result;
        }

        public static Array CreateArray(ObjectDescription arrayDescription, object item, int size = 1)
        {
            if (!arrayDescription.ClrType.IsArray)
                throw new ArgumentException("The given objectDescription is not an array");
            var dimensionsArray = new int[size >= 0 ? size : arrayDescription.ClrType.GetArrayRank()];
            var array = Array.CreateInstance(arrayDescription.ClrType.GetGenericArguments()[0], dimensionsArray);
            for (var i = 0; i < size; i++)
                array.SetValue(item, i);
            return array;
        }

        /// <summary>
        /// Create 1D array
        /// </summary>
        /// <param name="type">items type</param>
        /// <param name="item">the default value to fill the array with</param>
        /// <param name="size">the length of the array</param>
        /// <returns>A 1D array with length "size" filled with "item"</returns>
        /// <exception cref="ArgumentException">If "type" is not an array type (not a T[])</exception>
        public static Array CreateArray(Type type, Type item, int size = 1)
        {
            if (!type.IsArray)
                throw new ArgumentException("The given objectDescription is not an array");
            var dimensionsArray = new int[size >= 0 ? size : type.GetArrayRank()];
            var array = Array.CreateInstance(type.GetElementType(), dimensionsArray);
            for (var i = 0; i < size; i++)
                array.SetValue(item, i);
            return array;
        }

        public static Array CreateArray(Type type, Type item, int[] dimensions)
        {
            if (!type.IsArray)
                throw new ArgumentException("The given objectDescription is not an array");
            if (dimensions.Length != type.GetArrayRank())
                throw new ArgumentOutOfRangeException(nameof(dimensions));
            var array = Array.CreateInstance(type.GetElementType(), dimensions);
            var indices = Array.CreateInstance(typeof(int), dimensions.Length) as int[];
            for (var dimension = 0; dimension < dimensions.Length; dimension++)
                for (var i = 0; i < dimensions[dimension]; ++i)
                    //это на самом деле не правильно, надо доделать
                    array.SetValue(item, indices);
            return array;
        }

        public static Array CreateArrayEmpty(Type type, int size = 1)
        {
            if (!type.IsArray)
                throw new ArgumentException("The given objectDescription is not an array");
            var dimensionsArray = new int[size >= 0 ? size : type.GetArrayRank()];
            for (var i = 0; i < size; i++)
                dimensionsArray[i] = 0;
            var array = Array.CreateInstance(type.GetElementType(), dimensionsArray);
            return array;
        }

        public static IList<TResult> Map<TInput, TResult>(PropertyDescription.Func<TInput, TResult> map, TInput[] input)
        {
            var list = new List<TResult>();
            for (var i = 0; i < input.Length; i++)
                list.Add(map(input[i]));
            return list;
        }

        internal CollectionDescription(ObjectDescription owningType)
        {
            OwningType = owningType;
        }

        public ObjectDescription OwningType { get; }

        public bool IsGeneric => throw new NotImplementedException();

        public IDomain ElementType => throw new NotImplementedException();

        public bool CanBeEmpty => throw new NotImplementedException();

        public bool IsList => throw new NotImplementedException();

        public bool IsArray => throw new NotImplementedException();

        public bool IsResizable => throw new NotImplementedException();

        public bool IsMutable => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string Namespace => throw new NotImplementedException();

        public string Source => throw new NotImplementedException();

        public string FullName => throw new NotImplementedException();

        public string FullyQualifiedName => throw new NotImplementedException();


        public IEnumerable Create(object item, int count = 1)
        {
            throw new NotImplementedException();
        }

        public IEnumerable CreateEmpty()
        {
            throw new NotImplementedException();
        }

        public int GetDimensionCount(object collection)
        {
            throw new NotImplementedException();
        }

        public object GetElement(int index)
        {
            throw new NotImplementedException();
        }

        public object GetElement(int[] indices)
        {
            throw new NotImplementedException();
        }

        public int GetElementCount(object collection)
        {
            throw new NotImplementedException();
        }

        public void SetElement(int index)
        {
            throw new NotImplementedException();
        }

        public void SetElement(int[] indices)
        {
            throw new NotImplementedException();
        }

        public bool TryGetElement(int index, out object element)
        {
            throw new NotImplementedException();
        }

        public bool TryGetElement(int[] indices, out object element)
        {
            throw new NotImplementedException();
        }
    }

    public class ComplexObjectDescription : ObjectDescription, IDomainStructureInfo
    {
        public static ComplexObjectDescription GetConstructorDescription(Type type)
        {
            var od = ObjectDescription.Get(type);
            if ( !od.IsComplex )
                throw new ArgumentException("The supplied objectDescription is not a constuctor");
            else
                //TODO: Переделать; отделить от ObjectDescription (?)
                return od as ComplexObjectDescription;
        }

        public static ComplexObjectDescription GetConstructorDescription<T>()
        {
            return GetConstructorDescription(typeof (T));
        }

        private int _constructorArity;
        private SubObjectInfoCollection _subObjects;

        //TODO: Сделать поддержку Generic типов
        protected internal ComplexObjectDescription() 
        {
        }

        protected internal new void Init(Type type)
        {
            //TODO:Допроектировать использование интерфейса IObjectConstructor
            if ( IsSubtypeOf(typeof(IObjectConstructor)) )
                throw new NotImplementedException(); // Как вычислять кардинальность ? Требуются зависимые типы ?
            else
            {
                var subObjects = new List<SubObjectInfo>();
                var subObjectProperties = SubObjectInfoCollection.GetSubObjectProperties(Properties);
                foreach ( var pair in subObjectProperties )
                    subObjects.Add(new SubObjectInfo(pair.Key, pair.Value));
                var constructorArity = subObjects.Count;

                _subObjects = new SubObjectInfoCollection(subObjects);
                _constructorArity = constructorArity;
            }
        }

        public int ConstructorArity
        {
            get { return _constructorArity; }
        }

        public SubObjectInfoCollection SubObjects => _subObjects;

        public object[] GetSubObjects(object o)
        {
            if (Get(o.GetType()).IsSubtypeOf(this))
            {
                var result = new object[_subObjects.Count];
                for (var i = 0; i < _subObjects.Count; i++)
                    result[i] = _subObjects[i].GetSubObject(o);
                return result;
            }
            throw new ArgumentException("Invalid object objectDescription");
        }

        public bool IsContainer => throw new NotImplementedException();

        public bool IsMultipartJoint => throw new NotImplementedException();

        IDomain IStructureInfo<IDomain, IDomainConstructor>.ReflectedDomain => throw new NotImplementedException();

        IDomainConstructor IStructureInfo<IDomain, IDomainConstructor>.Constructor => throw new NotImplementedException();

        IList<IDomain> IStructureInfo<IDomain, IDomainConstructor>.Components => throw new NotImplementedException();

        // IEnumerable<ITerm> ICompositeTerm.SubTerms => throw new NotImplementedException();
        //
        // ITff ICompositeTerm.GetTff => throw new NotImplementedException();
        //
        // IEnumerable<IVariable> ICompositeTerm.GetBoundVariables => throw new NotImplementedException();
    }

    public abstract class SubObjectInfoBase<T>
    {
        //TODO: вернуть internal
        protected SubObjectInfoBase(T subObject, string role, int index)
        {
            SubObject = subObject;
            Role = role;
            Index = index;
        }

        public T SubObject { get; }

        public string Role { get; }

        public int Index { get; }

        public int Cardinality => throw new NotImplementedException();

        public abstract object GetSubObject(object o);
    }

    public class SubObjectInfo : SubObjectInfoBase<ObjectDescription>
    {
        public SubObjectInfo(PropertyDescription property, int index) : base(property.Range, property.Name, index)
        {
            UnderlyingProperty = property;
        }

        //TODO: вернуть internal
        public PropertyDescription UnderlyingProperty { get; }

        public override object GetSubObject(object o)
        {
            if (!ObjectDescription.Get(o.GetType()).IsSubtypeOf(UnderlyingProperty.Domain))
                throw new ArgumentException("Invalid object objectDescription");
            return UnderlyingProperty.GetValue(o);
        }
    }

    public class SubObjectInfoCollectionBase<TInfo,TDescription> : IEnumerable<TInfo> where TInfo : SubObjectInfoBase<TDescription>
    {
        internal static IList<KeyValuePair<PropertyDescription, int>> GetSubObjectProperties(IEnumerable<PropertyDescription> allProperties)
        {
            var selectedProperties = new List<KeyValuePair<PropertyDescription, int>>();
            foreach (var property in allProperties)
            {
                var attribute = property.GetAttribute<SubObjectAttribute>();
                //TODO: добавить валидацию -- свойство должно быть только для чтения (?)
                if (attribute != null)
                    selectedProperties.Add(new KeyValuePair<PropertyDescription, int>(property, attribute.Index));
            }
            selectedProperties.Sort((e1, e2) => e1.Value.CompareTo(e2.Value));

            List<int> GetOccupiedIndices(List<KeyValuePair<PropertyDescription, int>> list)
            {
                var indices = new List<int>();
                for (var i = list.Count - 1; i >= 0; i--)
                    if (list[i].Value >= 0)
                        indices.Add(list[i].Value);
                    else
                        break;
                return indices;
            }

            bool AreAllIndicesWithinRange(List<int> list)
            {
                foreach (var i in list)
                    if (i >= selectedProperties.Count)
                        return false;
                return true;
            }

            var occupiedIndices = GetOccupiedIndices(selectedProperties);
            if (!AreAllIndicesWithinRange(occupiedIndices))
                throw new ArgumentException("");

            var currentItemIndex = 0;
            var currentSubObjectIndex = 0;
            while (currentItemIndex < selectedProperties.Count && selectedProperties[currentItemIndex].Value < 0)
            {
                while (occupiedIndices.Contains(currentSubObjectIndex))
                    currentSubObjectIndex++;
                selectedProperties[currentItemIndex] = new KeyValuePair<PropertyDescription, int>(
                                    selectedProperties[currentItemIndex].Key, currentSubObjectIndex);
                occupiedIndices.Add(currentSubObjectIndex);
                currentItemIndex++;
            }

            selectedProperties.Sort((e1, e2) => e1.Value.CompareTo(e2.Value));
            return selectedProperties;
        }

        private readonly List<TInfo> _subObjectInfos;
        private readonly Dictionary<string, TInfo>  _byName = new Dictionary<string, TInfo>();

        protected SubObjectInfoCollectionBase(IEnumerable<TInfo> objectInfos)
        {
            _subObjectInfos = new List<TInfo>(objectInfos);
            _subObjectInfos.Sort((x1, x2) => Math.Abs(x1.Index - x2.Index));
            foreach (var subObjectInfo in _subObjectInfos)
                _byName.Add(subObjectInfo.Role, subObjectInfo);
        }

        public TInfo this[int index] => _subObjectInfos[index];

        public TInfo this[string name] => _byName[name];

        public int Count => _subObjectInfos.Count;

        public IEnumerator<TInfo> GetEnumerator()
        {
            return _subObjectInfos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class SubObjectInfoCollection : SubObjectInfoCollectionBase<SubObjectInfo, ObjectDescription>
    {
        internal SubObjectInfoCollection(IEnumerable<SubObjectInfo> objectInfos) : base(objectInfos)
        {
        }
    }

    public class FunctionDescription : IFunctionTypeInfo
    {
        private static readonly Dictionary<ObjectDescription,FunctionDescription>  _byObjectDescriptions = new Dictionary<ObjectDescription, FunctionDescription>();

        public static bool IsFunction(ObjectDescription objectDescription)
        {
            return objectDescription.IsSubtypeOf(ObjectDescription.DelegateDescription);
        }

        public static FunctionDescription GetFunctionDescription(ObjectDescription objectDescription)
        {
            if ( IsFunction(objectDescription) )
            {
                lock ( _byObjectDescriptions )
                    if ( !_byObjectDescriptions.ContainsKey(objectDescription) )
                        _byObjectDescriptions.Add(objectDescription, new FunctionDescription(objectDescription));
                return _byObjectDescriptions[objectDescription];
            }
            else
                return null;
        }

        private readonly ReadOnlyCollection<ObjectDescription> _arguments;
        private readonly ObjectDescription _returnType;

        protected internal FunctionDescription(ObjectDescription objectDescription)
        {
            if (objectDescription.ClrType == typeof(Delegate) || objectDescription.IsSubtypeOf(ObjectDescription.DelegateDescription))
            {
                var invokeMethodInfo = objectDescription.ClrType.GetMethod("Invoke");
                _arguments = new ReadOnlyCollection<ObjectDescription>(new List<ObjectDescription>(
                    CollectionDescription.Map(p => ObjectDescription.Get(p.ParameterType), invokeMethodInfo.GetParameters())));
                _returnType = ObjectDescription.Get(invokeMethodInfo.ReturnType);
            }
            else
                throw new ArgumentException("Must be a delegate objectDescription");



//            _arguments = new ReadOnlyCollection<ObjectDescription>();


        }

        public ICollection<ObjectDescription> Arguments => _arguments;

        public ObjectDescription Range => _returnType;

        public int Arity => _arguments.Count;

        public bool IsCurried => false;

        #region IFunctionTypeInfo
        IDomain IFunctionTypeInfo.ReflectedDomain
        {
            get { throw new NotImplementedException(); }
        }

        object IFunctionTypeInfo.Apply(object function, IList<object> args)
        {
            throw new NotImplementedException();
        }

        object IFunctionTypeInfo.Apply(object function, object[] args)
        {
            throw new NotImplementedException();
        }

        Delegate IFunctionTypeInfo.AsDelegate
        {
            get { throw new NotImplementedException(); }
        }

        bool IFunctionTypeInfo.IsDelegateConformant(Delegate functionDelegate)
        {
            throw new NotImplementedException();
        }

        IDomain IFunctionSignature<IDomain, IArgumentInfo<IDomain>>.ReturnType
        {
            get { throw new NotImplementedException(); }
        }

        IArgumentInfo<IDomain>[] ISignature<IDomain, IArgumentInfo<IDomain>>.Arguments
        {
            get { throw new NotImplementedException(); }
        }

        IArgumentInfo<IDomain> ISignature<IDomain, IArgumentInfo<IDomain>>.this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        IArgumentInfo<IDomain> ISignature<IDomain, IArgumentInfo<IDomain>>.this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        int ISignature<IDomain, IArgumentInfo<IDomain>>.Arity
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }

}