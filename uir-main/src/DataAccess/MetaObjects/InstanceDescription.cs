using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects
{
    public class InstanceDescription
    {
        private object _describedInstance;
        private EntityDescription _entityDescription;
        private bool _isEntity;
        private bool _isEntityBase;
        private List<InstancePropertyDescription> _properties = new List<InstancePropertyDescription>();

        internal InstanceDescription(object instance)
        {
            _describedInstance = instance;
            _entityDescription = EntityDescription.GetEntityDescription(instance.GetType());
            if ( _entityDescription == null )
                throw new ArgumentException();
            //_entityDescription = EntityFactory.GetObjectDescription(_describedInstance.GetType(),
            //        EntityFactory.GetThreadDefaultPs(System.Threading.Thread.CurrentThread)); //new EntityDescription(_describedInstance.GetType());
            _isEntity = EntityDescription.IsEntity(instance.GetType());
            _isEntityBase = EntityDescription.IsEntityBase(_entityDescription);
        }

        public object DescribedInstance
        {
            get { return _describedInstance; }
        }

        public EntityDescription EntityDescription
        {
            get { return _entityDescription; }
        }

        public bool IsEntity
        {
            get { return _isEntity; }
        }

        public bool IsEntityBase
        {
            get { return _isEntityBase; }
        }

        public List<InstancePropertyDescription> Properties
        {
            get { return _properties; }
        }

        //public List<IEntity> GetRelatedOjects(PropertyDescription property)
        //{
        //    if ( !IsEntityBase )
        //        throw new ApplicationException("The DescribedInstance should be an instance of a type derived from Entity");
        //    if ( !property.IsRelation )
        //        throw new ApplicationException("The property should be a relation");
        //    return ( DescribedInstance as EntityBase ).PersistentStorage.GetEntities(property.RelatedType);
        //}
    }
}
