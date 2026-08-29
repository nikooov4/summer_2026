using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using System.Data;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.Attributes;
using Mephi.Cybernetics.Race.Core.Attributes.ComplexObjects;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects
{
    /// <summary>
    /// Provides the entity-relationship description of data objects
    /// </summary>
    [Serializable]
    public class EntityDescription : IDomain
    {
        private static readonly Dictionary<Type, EntityDescription> _byType = new Dictionary<Type, EntityDescription>();
        private static readonly Dictionary<ObjectDescription, EntityDescription> _byObjectDescription = new Dictionary<ObjectDescription, EntityDescription>();

        public static readonly ObjectDescription IEntityObjectDscription = ObjectDescription.Get(typeof(IEntity));
        public static readonly ObjectDescription EntityBaseObjectDescription = ObjectDescription.Get(typeof(EntityBase));

        public static bool IsEntity(ObjectDescription objectDescription)
        {
            return objectDescription.GetClrAttribute<EntityAttribute>() != null || objectDescription.IsSubtypeOf(IEntityObjectDscription);
        }

        public static bool IsEntity(Type type)
        {
            return IsEntity(ObjectDescription.Get(type));
        }

        public static bool IsEntityBase(EntityDescription entityDescription)
        {
            return entityDescription.Object.IsSubtypeOf(EntityBaseObjectDescription);
        }

        public static bool IsEntityBase(ObjectDescription objectDescription)
        {
            return objectDescription.IsSubtypeOf(EntityBaseObjectDescription);
        }

        public static bool IsEntityBase(Type type)
        {
            return IsEntityBase(ObjectDescription.Get(type));
        }

        public static EntityDescription GetEntityDescription(Type type)
        {
            lock (_byType)
                if ( !_byType.ContainsKey(type) )
                {
                    var objectDescription = ObjectDescription.Get(type);
                    var entityDescription = objectDescription.IsComplex ? new EntityConstructorDescription(objectDescription) : new EntityDescription(objectDescription);
                    _byType.Add(type, entityDescription);
                    _byObjectDescription.Add(objectDescription, entityDescription);
                    entityDescription.Init();
                }
            return _byType[type];
        }

        public static EntityDescription GetEntityDescription<T>()
        {
            return GetEntityDescription(typeof (T));
        }

        public static EntityDescription GetEntityDescription(ObjectDescription objectDescription)
        {
            lock ( _byType )
                if ( !_byObjectDescription.ContainsKey(objectDescription) )
                {
                    var entityDescription = objectDescription.IsComplex ? new EntityConstructorDescription(objectDescription) : new EntityDescription(objectDescription);
                    _byObjectDescription.Add(objectDescription, entityDescription);
                    _byType.Add(objectDescription.ClrType, entityDescription);
                    entityDescription.Init();
                }
            return _byObjectDescription[objectDescription];
        }

        private readonly string _name;
        protected readonly ObjectDescription _objectDescription;
        private EntityDescription[] _parents;
//        private readonly EntityDescription _allParentEntities;
        private EntityAttributeDescription _systemId;
        private EntityAttributeDescription _primaryKey;
        private bool _isAggregated;
        private EntityDescription[] _aggregatedTypes;
        private Type _idFieldType;
        private bool _isWrapped;
        private Type _wrappedClass;
        private bool _isVirtual;
        private bool _isDenyGetAllOnGetById = false;
        private string _getAllMethodName;
        private readonly TableAttribute _tableAttribute = null;
        private readonly EntityAttributesCollection _entityAttributes = new EntityAttributesCollection();
        private bool _isAtomic;
        private bool _isComplex;


        internal EntityDescription(ObjectDescription objectDescription)
        {
            if (!IsEntity(objectDescription))
                throw new ArgumentException("The specified ObjectDescription is not an entity");

            _objectDescription = objectDescription;
            _name = _objectDescription.Name;


        }

        protected internal virtual void Init()
        {
            foreach (var property in Object.Properties)
            {
                if (!(property.HasAttribute<InternalAttribute>() || property.HasAttribute<SubObjectAttribute>()))
                {
                    var entityAttribute = new EntityAttributeDescription(this, property);
                    EntityAttributes.AddProperty(entityAttribute);

                    if (entityAttribute.IsId)
                        _systemId = entityAttribute;
                }
            }

            _parents = GetParents();

            _isAtomic = !(_isComplex = _objectDescription.IsComplex);

            _isAggregated = _objectDescription.HasClrAttribute<AgregatedClassAttribute>();
            // _aggregatedTypes ����������� � ��������, ����� �������� ��������, ����� ������ ��� ��������
            // ���������������, � ��� �������� �������� ������������


            if (_objectDescription.HasClrAttribute<PrimaryKeyAttribute>())
                _primaryKey = this.EntityAttributes[_objectDescription.GetClrAttribute<PrimaryKeyAttribute>().Name];
            else
                _primaryKey = _systemId;

            var idTypeAttr = _objectDescription.GetClrAttribute<IdFieldTypeAttribute>();
            if (idTypeAttr == null)
                _idFieldType = _primaryKey == null
                                   ? null
                                   : (_primaryKey.ObjectProperty.PropertyType == typeof(object)
                                          ? typeof(Guid)
                                          : _primaryKey.ObjectProperty.PropertyType);
            else
            {
                if (PrimaryKey.ObjectProperty.PropertyType.IsAssignableFrom(idTypeAttr.IdType))
                    _idFieldType = idTypeAttr.IdType;
                else
                    throw new Exception(
                        string.Format(
                            "Invalid primary key type resctriction: specified type '{1}' is incompatible with the primary key filed '{2}' of type '{0}'",
                            PrimaryKey.ObjectProperty.PropertyType, idTypeAttr.IdType, PrimaryKey.Name));
            }


            var wattr = _objectDescription.GetClrAttribute<WrappedClassAttribute>();
            if (wattr != null)
            {
                if (!_objectDescription.IsSubtypeOf(typeof(IWrapObject)))
                    throw new Exception("A class marked with WrappedClass attribute must implement the IWrapObject interface");
                _isWrapped = true;
                _wrappedClass = wattr.WrappedType;
            }
            else
                _isWrapped = false;

            var vattr = _objectDescription.GetClrAttribute<VirtualObjectAttribute>();
            if (vattr != null)
            {
                _isVirtual = true;
                _getAllMethodName = vattr.GetAllMethodName;
            }

            _isDenyGetAllOnGetById = _objectDescription.GetClrAttribute<DenyLoadAllOnGetByIdAttribute>() != null;
        }

        private EntityDescription[] GetParents()
        {
            var list = new List<EntityDescription>();
            foreach ( var objectDescription in _objectDescription.Parents )
                if (EntityDescription.IsEntity(objectDescription))
                    list.Add(EntityDescription.GetEntityDescription(objectDescription));
            return list.ToArray();
        }

        public string Name => _name;

        public string Namespace => _objectDescription.Namespace;

        public string Source => _objectDescription.Source;

        public string FullName => _objectDescription.FullName;

        public string FullyQualifiedName => _objectDescription.FullyQualifiedName;

        public bool IsAtomic => _isAtomic;

        public bool IsComplex => _isComplex;


        public EntityAttributesCollection Attributes => _entityAttributes;

        public Type IdFieldType => _idFieldType;

        public EntityAttributeDescription SystemId => _systemId;

        public EntityAttributeDescription PrimaryKey => _primaryKey;

        public List<EntityConstraintAttribute> CheckConstraints(IEntity entity, Dictionary<string, object> values)
        {
            var errors = new List<EntityConstraintAttribute>();
            foreach (var attribute in _objectDescription.ClrAttributes)
            {
                if ( attribute is EntityConstraintAttribute )
                {
                    var constraint = attribute as EntityConstraintAttribute;
                    if ( !constraint.CheckEntity(entity, values) )
                        errors.Add(constraint);
                }
            }
            return errors;
        }

        public bool IsDenyGetAllOnGetById => _isDenyGetAllOnGetById;

        public bool IsWrapped => _isWrapped;

        public Type WrappedClass => _wrappedClass;

        public bool IsAggregated => _isAggregated;

        public EntityDescription[] AggregatedTypes
        {
            get
            {
                if (_aggregatedTypes == null) // temporarily
                {
                    _aggregatedTypes =
                        Array.ConvertAll(_objectDescription.GetClrAttribute<AgregatedClassAttribute>().ChildTypes,
                                                                  EntityDescription.GetEntityDescription);
                }
                return _aggregatedTypes;
            }
        }

        public bool IsVirtual
        {
            get => _isVirtual;
            set => _isVirtual = value;
        }

        public string GetAllMethodName
        {
            get => _getAllMethodName;
            set => _getAllMethodName = value;
        }

        public TableAttribute DbTableAttribute => _tableAttribute;

        public EntityAttributesCollection EntityAttributes => _entityAttributes;

        public ObjectDescription Object => _objectDescription;

        public EntityDescription[] Parents => _parents;


        //public bool SupportsProperty(PropertyDescription property)
        //{
        //    if ( Properties.Count > property.Index )
        //    {
        //        var localProperty = Properties[property.Index];
        //        return localProperty.Name == property.Name && localProperty.PropertyType == property.PropertyType  && localProperty
        //    }
        //    else
        //        return false;
        //}

        public object CreateObject()
        {
            return _objectDescription.CreateObject();
        }

        public override string ToString()
        {
            return $"Type: {this.Object.Name}";
        }
    }

    public class EntityConstructorDescription : EntityDescription
    {
        public static EntityConstructorDescription GetConstructorDescription(Type type)
        {
            var od = EntityDescription.GetEntityDescription(type);
            if (!od.IsComplex)
                throw new ArgumentException("The supplied type is not a constructor");
            else
                return od as EntityConstructorDescription;
        }


        public static EntityConstructorDescription GetConstructorDescription<T>()
        {
            return GetConstructorDescription(typeof(T));
        }

        private int _constructorArity;
        private SubEntityInfoCollection _subEntities;
        
        public EntityConstructorDescription(ObjectDescription objectDescription) : base(objectDescription)
        {
        }

        protected internal override void Init()
        {
            base.Init();
            if (!_objectDescription.IsComplex)
                throw new ArgumentException("The ObjectDescription object must a constructor description");
            var complex = _objectDescription as ComplexObjectDescription;
            _constructorArity = complex.ConstructorArity;
            var subEntities = new List<SubEntityInfo>();
            foreach (var subObject in complex.SubObjects)
            {
                if (EntityDescription.IsEntity(subObject.SubObject) /*&& 
                        subObject.UnderlyingProperty.GetClrAttribute<SubEntityAttribute>() != null*/ )
                    subEntities.Add(new SubEntityInfo(subObject));
            }
            _subEntities = new SubEntityInfoCollection(subEntities);
        }

        public int ConstructorArity => _constructorArity;

        public SubEntityInfoCollection SubEntities => _subEntities;
    }

    public class SubEntityInfo : SubObjectInfoBase<EntityDescription>
    {
        private readonly PropertyDescription _property;

        public SubEntityInfo(SubObjectInfo subObject) 
            : base(EntityDescription.GetEntityDescription(subObject.UnderlyingProperty.Domain), subObject.UnderlyingProperty.Name, subObject.Index)
        {
            _property = subObject.UnderlyingProperty;
        }

        public override object GetSubObject(object o)
        {
            if (!ObjectDescription.Get(o.GetType()).IsSubtypeOf(_property.Domain))
                throw new ArgumentException("Invalid object type");
            return _property.GetValue(o);
        }
    }

    public class SubEntityInfoCollection : SubObjectInfoCollectionBase<SubEntityInfo, EntityDescription>
    {
        internal SubEntityInfoCollection(IEnumerable<SubEntityInfo> objectInfos) : base(objectInfos)
        {
        }
    }
}
