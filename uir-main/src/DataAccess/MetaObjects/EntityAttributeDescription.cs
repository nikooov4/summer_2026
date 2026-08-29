using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.Attributes;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects
{
    public class EntityAttributeDescription : IDomainAttribute
    {
        private readonly PropertyDescription _propertyDescription;
        private readonly EntityDescription _domain;
        private readonly EntityDescription _relatedEntity;
        private readonly bool _isInternal;
        private readonly bool _isRelation;
        private readonly bool _isOneToOneRelation;
        private readonly bool _isOneToManyRelation;
        private readonly bool _isManyToManyRelation;
        private readonly bool _isId;
        private readonly bool _isNonPersistent;
        private readonly RelationAttribute _relationAttribute;
        private readonly bool _isObligatory;
        private readonly PropertyDescription.Func<object, object> _getValueWithoutChecks;
        private readonly PropertyDescription.Func<object, object> _getValue;
        private readonly List<FieldConstraintAttribute> _propertyConstraints = new List<FieldConstraintAttribute>();

        public EntityAttributeDescription(EntityDescription entityDescription, PropertyDescription propertyDescription)
        {
            _propertyDescription = propertyDescription;
            _domain = entityDescription;
            foreach (var attr in _propertyDescription.Attributes)
            {
                if (attr is InternalAttribute)
                    _isInternal = true;
                if (attr is RelationAttribute)
                {
                    _isRelation = true;
                    _relationAttribute = (RelationAttribute)attr;
                    if ((attr as RelationAttribute).RelationType == RelationType.OneToOne)
                        _isOneToOneRelation = true;
                    if ((attr as RelationAttribute).RelationType == RelationType.OneToMany)
                        _isOneToManyRelation = true;
                    if ((attr as RelationAttribute).RelationType == RelationType.ManyToMany)
                        _isManyToManyRelation = true;
                    //                    _relatedType = _relationAttribute.RelatedType;
                }
                if (attr is NonPersistentAttribute)
                    _isNonPersistent = true;
                if (attr is IdFieldAttribute)
                    _isId = true;
                if (attr is ObligatoryFieldAttribute)
                    _isObligatory = true;
                if (IsReadOnly)
                    _isObligatory = true;
                if (attr is FieldConstraintAttribute)
                    _propertyConstraints.Add(attr as FieldConstraintAttribute);
            }

            if (_isRelation) {
                if (_propertyDescription.RangeType.IsGenericType)
                    _relatedEntity = EntityDescription.GetEntityDescription(_propertyDescription.RangeType.GetGenericArguments()[0]);
                else
                    _relatedEntity = EntityDescription.GetEntityDescription(_propertyDescription.Range);
            }
            else {
                _relatedEntity = null;
            }

            // _relatedEntity = _isRelation ? EntityDescription.GetEntityDescription(_propertyDescription.Range) : null;

            // ������� ������� _getValueWithoutChecks, ������� �������� �������� �������� "������" (�������������� ��������, ����� ��� ������� ��� ��, ��� � ��� ������� ��������)
            if (!IsComputable)
            {
                if (EntityDescription.IsEntityBase(_domain))
                    _getValueWithoutChecks = o => (o as EntityBase)[this, false];
                else if (_domain.Object.IsSubtypeOf(EntityDescription.IEntityObjectDscription))
                    _getValueWithoutChecks = o => (o as IEntity)[this];
                else
                    _getValueWithoutChecks = _propertyDescription.PropertyGetter;
            }
            else
            {
                if (EntityDescription.IsEntityBase(_domain) && _propertyDescription.ComputableInfo.AllowCaching)
                    _getValueWithoutChecks = o =>
                    {
                        var v = (o as EntityBase)[this, false];
                        if (v is UnasignedValue)
                            return (o as EntityBase)[this, false] = _propertyDescription.PropertyGetter(o);
                        else
                            return v;
                    };
                else if (_domain.Object.IsSubtypeOf(EntityDescription.IEntityObjectDscription) && _propertyDescription.ComputableInfo.AllowCaching)
                    _getValueWithoutChecks = o =>
                    {
                        var v = (o as IEntity)[this];
                        if (v is UnasignedValue)
                            return (o as IEntity)[this] = _propertyDescription.PropertyGetter(o);
                        else
                            return v;
                    };
                else
                    _getValueWithoutChecks = _propertyDescription.PropertyGetter;
            }

            // ������ ������� �������� _getValue, ������� ��������� ������������ � �������������
            //TODO: �����������; ��������, ��� ��� �� ���
            if ( !_propertyDescription.IsOverloadable )
            {
                _getValue = _getValueWithoutChecks;
            }
            else //if (IsOverloadable)
            {
                if ( this.Domain.Object.ClrType.IsInterface )
                {
                    _getValue = o =>
                    {
                        //TODO: �������� �������, ��� ���� �������� �������� ���������, �� ������������ _getValueWithoutChecks
                        var od = ObjectDescription.Get(o.GetType());
                        return od.InterfaceMapping[_propertyDescription].GetValue(o); //_getValueWithoutChecks(o);
                    };
                }
                else
                    _getValue = o =>
                    {
                        //TODO: �������� �������, ��� ���� �������� �������� ���������, �� ������������ _getValueWithoutChecks
                        var od = ObjectDescription.Get(o.GetType());
                        return od.Properties[_propertyDescription.Index].GetValue(o);
                    };
            }


        }

        public string Name
        {
            get { return _propertyDescription.Name; }
        }

        IDomain IAttributeBase2<IDomain, IDomain>.Domain => _domain;

        IDomain IAttributeBase2<IDomain, IDomain>.Range => Range;

        public EntityDescription Domain => _domain;

        public ObjectDescription Range => _propertyDescription.Range;

        public EntityDescription RelatedEntity => _relatedEntity;

        public bool IsComputable => _propertyDescription.IsComputable;

        public PropertyDescription ObjectProperty => _propertyDescription;

        public object GetValue(object o)
        {
            return _getValue(o);
        }

        public T GetValue<T>(object o)
        {
            return (T)_getValue(o);
        }

        public void SetValue<T>(object o, T value)
        {
            SetValue(o, value as object);
        }

        public void SetValue(object o, object value)
        {
            _propertyDescription.SetValue(o, value);
        }

        public bool IsInternal
        {
            get { return _isInternal; }
        }

        public bool IsRelation
        {
            get { return _isRelation; }
        }

        public bool IsOneToOneRelation
        {
            get { return _isOneToOneRelation; }
        }

        public bool IsOneToManyRelation
        {
            get { return _isOneToManyRelation; }
        }

        public bool IsManyToManyRelation
        {
            get { return _isManyToManyRelation; }
        }

        public bool IsId
        {
            get { return _isId; }
        }

        public bool IsMapped
        {
            get { return !IsInternal && !IsNonPersistent && !IsManyToManyRelation && !IsOneToManyRelation; }
        }

        public bool IsNonPersistent
        {
            get { return _isNonPersistent || IsComputable; }
        }

        public bool IsObligatory
        {
            get { return _isObligatory; }
        }

        public bool IsReadOnly
        {
            get { return _propertyDescription.IsReadOnly; }
        }

        public RelationAttribute RelationInfo
        {
            get { return _relationAttribute; }
        }

        public List<FieldConstraintAttribute> CheckConstraints(IEntity entity, object value)
        {
            if (entity.GetType() != this.Domain.Object.ClrType)
                throw new ArgumentException(string.Format("entity type '{0}' does not match property type '{1}'",
                                                          entity.GetType(), this.Domain.Object.ClrType));
            var failedConstraints = new List<FieldConstraintAttribute>();
            foreach ( var constraint in _propertyConstraints )
            {
                if ( !constraint.CheckValue(entity, this, value) )
                    failedConstraints.Add(constraint);
            }
            return failedConstraints;
        }
    }
}