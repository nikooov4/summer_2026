using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mephi.Cybernetics.Race.Core.Attributes;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Dyadic
{
    public class EntityAssociationCollection : IEntityAssociationCollection
    {
        private readonly List<EntityAssociation> _entityAssociations;

        private readonly ErdEntity _entity;
        public static readonly ObjectDescription IEntityObjectDscription = ObjectDescription.Get(typeof(IEntity));

        internal EntityAssociationCollection(IList<IEntityAssociation> o2o, IList<IEntityAssociation> o2m,
            IList<IEntityAssociation> m2m)
        {
            OneToOne = new EntityOneToOneAssociationCollection(o2o, _entity);
            OneToMany = new EntityOneToManyAssociationCollection(o2m, _entity);
            ManyToMany = new EntityManyToManyAssociationCollection(m2m, _entity);
        }

        public IEntityAssociation this[int index]
        {
            get
            {
                IEntityAssociation res;
                var o2oCount = Enumerable.Count(OneToOne);
                var o2mCount = Enumerable.Count(OneToMany);
                var m2mCount = Enumerable.Count(ManyToMany);
                if (o2oCount > index)
                {
                    res = OneToOne[index];
                }
                else if (o2oCount + o2mCount > index)
                {
                    res = OneToMany[index];
                }
                else if (o2oCount + o2mCount + m2mCount > index)
                {
                    res = ManyToMany[index];
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }

                return res;
            }
        }

        public IEntityAssociation this[string name] =>
            OneToOne.Contains(name)
                ? OneToOne[name]
                : (OneToMany.Contains(name)
                    ? OneToMany[name]
                    : (ManyToMany.Contains(name)
                        ? ManyToMany[name]
                        : null));

        public IEntityOneToOneAssociationCollection OneToOne { get; }

        IEntityOneToOneAssociationCollection IEntityAssociationCollection.OneToOne => this.OneToOne;

        public IEntityOneToManyAssociationCollection OneToMany { get; }

        IEntityOneToManyAssociationCollection IEntityAssociationCollection.OneToMany => this.OneToMany;

        public IEntityManyToManyAssociationCollection ManyToMany { get; }

        IEntityManyToManyAssociationCollection IEntityAssociationCollection.ManyToMany => this.ManyToMany;

        public IEntity ReflectedDomain => throw new NotImplementedException();

        public bool Contains(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IEntityAssociation> GetEnumerator()
        {
            return new SwitchableEnumerator<IEntityAssociation>(OneToOne, OneToMany, ManyToMany);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SwitchableEnumerator<IEntityAssociation>(OneToOne, OneToMany, ManyToMany);
        }
    }

    public class EntityOneToOneAssociationCollection : IEntityOneToOneAssociationCollection
    {
        private IList<IEntityAssociation> OneToOne;
        private Dictionary<string,IEntityAssociation> _oneToOneByName;
        private ErdEntity _erdEntity;
        public EntityOneToOneAssociationCollection(IList<IEntityAssociation> o2o, ErdEntity erdEntity)
        {
            OneToOne = o2o;
            _erdEntity = erdEntity;
            _oneToOneByName = new Dictionary<string, IEntityAssociation>();
            foreach (var assoc in o2o)
            {
                _oneToOneByName.Add(assoc.Name,assoc);
            }
        }

        public IEnumerator<IEntityAssociation> GetEnumerator()
        {
            return OneToOne.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEntity ReflectedDomain { get; }

        public IEntityAssociation this[int index] => OneToOne[index];

        public IEntityAssociation this[string name] => _oneToOneByName[name];

        public bool Contains(string name)=> _oneToOneByName.ContainsKey(name);
        
    }

    public class EntityOneToManyAssociationCollection : IEntityOneToManyAssociationCollection
    {
        private IList<IEntityAssociation> OneToMany;
        private Dictionary<string,IEntityAssociation> _oneToManyByName;
        private ErdEntity _erdEntity;
        
        public EntityOneToManyAssociationCollection(IList<IEntityAssociation> o2m, ErdEntity erdEntity)
        {
            OneToMany = o2m;
            _erdEntity = erdEntity;
            _oneToManyByName = new Dictionary<string, IEntityAssociation>();
            foreach (var assoc in o2m)
            {
                _oneToManyByName.Add(assoc.Name,assoc);
            }
        }
        
        public IEnumerator<IEntityAssociation> GetEnumerator()
        {
            return OneToMany.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEntity ReflectedDomain { get; }

        public IEntityAssociation this[int index] => OneToMany[ index];

        public IEntityAssociation this[string name] => _oneToManyByName[name];

        public bool Contains(string name) => _oneToManyByName.ContainsKey(name);
        
    }

    public class EntityManyToManyAssociationCollection : IEntityManyToManyAssociationCollection
    {
        private IList<IEntityAssociation> ManyToMany;
        private Dictionary<string,IEntityAssociation> _manyToManyByName;
        private ErdEntity _erdEntity;
        public EntityManyToManyAssociationCollection(IList<IEntityAssociation> m2m, ErdEntity erdEntity)
        {
            ManyToMany = m2m;
            _erdEntity = erdEntity;
            _manyToManyByName = new Dictionary<string, IEntityAssociation>();
            foreach (var assoc in m2m)
            {
                _manyToManyByName.Add(assoc.Name,assoc);
            }
        }

        public IEnumerator<IEntityAssociation> GetEnumerator()
        {
            return ManyToMany.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEntity ReflectedDomain { get; }

        public IEntityAssociation this[int index] => ManyToMany[ index];

        public IEntityAssociation this[string name] => _manyToManyByName[name];

        public bool Contains(string name) => _manyToManyByName.ContainsKey(name);
    }
}