using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mephi.Cybernetics.Race.Core.TypeSystem
{
    public class RaceTypeSystem : IRaceTypeSystem
    {
        private readonly Dictionary<Type, ObjectDescription> _metaObjects = new Dictionary<Type, ObjectDescription>();
        private readonly IList<ObjectDescription> _primitiveTypesDescriptions;

        private readonly RaceType _typeAny;
        private readonly RaceType _typeByte;
        private readonly RaceType _typeShort;
        private readonly RaceType _typeInt;
        private readonly RaceType _typeLong;
        private readonly RaceType _typeBool;
        private readonly RaceType _typeChar;
        private readonly RaceType _typeString;


        public RaceTypeSystem()
        {
            var primitiveTypes = new List<Type>(new Type[]
                                                {
                                                    typeof(object),
                                                    typeof(ValueType),
                                                    typeof(Nullable),
                                                    typeof (byte),
                                                    typeof (Int16),
                                                    typeof (Int32),
                                                    typeof (Int64),
                                                    typeof (UInt16),
                                                    typeof (UInt32),
                                                    typeof (UInt64),
                                                    typeof (Char),
                                                    typeof (DateTime),
                                                    typeof (float),
                                                    typeof (double),
                                                    typeof (bool),
                                                    typeof (string),
                                                    typeof (Delegate),
                                                    typeof (Enum),
                                                    typeof(Attribute)
                                                });
            _primitiveTypesDescriptions = new ReadOnlyCollection<ObjectDescription>(new ObjectDescription[primitiveTypes.Count]);

            var primitiveTypeConstructors = new List<Type>(new Type[]
                                                           {
                                                               typeof(IEnumerable<>),
                                                               typeof(IList<>),
                                                               typeof(List<>),
                                                               typeof(ICollection<>),
                                                               typeof(int[]).GetGenericTypeDefinition()
                                                               
                                                           });

            _typeAny = AddType("Any", "System", "Race.Core", Array.Empty<RaceType>());
            _typeLong = AddType("Long", "System", "Race.Core", new[] { _typeAny }, typeof(Int64));
            _typeInt = AddType("Int", "System", "Race.Core", new[] { _typeAny }, typeof(Int32));
            _typeShort = AddType("Short", "System", "Race.Core", new[] { _typeAny }, typeof(Int16));
            _typeByte = AddType("Byte", "System", "Race.Core", new[] { _typeAny }, typeof(byte));
            _typeBool = AddType("Bool", "System", "Race.Core", new[] { _typeAny }, typeof(bool));
            _typeChar = AddType("Char", "System", "Race.Core", new[] { _typeAny }, typeof(char));
            _typeString = AddType("String", "System", "Race.Core", new[] { _typeAny }, typeof(string));
        }

        public RaceType TypeAny => _typeAny;

        public RaceType TypeUnit
        {
            get { throw new NotImplementedException(); }
        }

        public IList<RaceType> PrimaryTypes
        {
            get { throw new NotImplementedException(); }
        }

        public IList<IDomainConstructor> PrimaryTypeConstructors 
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsDerivedFromPrimaries(RaceType domain)
        {
            throw new NotImplementedException();
        }

        public bool IsSubtypeOf(RaceType domain1, RaceType domain2)
        {
            var supertypes = GetSupertypes(domain1);
            return supertypes.BinarySearch(domain2) >= 0;
        }

        public IDomainAttributeCollection GetDomainAttributes(RaceType domain)
        {
            throw new NotImplementedException();
        }

        public bool IsFunction(RaceType domain)
        {
            throw new NotImplementedException();
        }

        public IFunctionTypeInfo GetFunctionTypeInfo(RaceType domain)
        {
            throw new NotImplementedException();
        }

        public bool IsComplex(RaceType domain)
        {
            throw new NotImplementedException();
        }

        public IDomainStructureInfo GetStructureInfo(RaceType domain)
        {
            throw new NotImplementedException();
        }

        public bool IsCollection(RaceType domain)
        {
            throw new NotImplementedException();
        }

        public ICollectionDomain GetCollectionInfo(RaceType domain)
        {
            throw new NotImplementedException();
        }


        #region IRaceTypeSystem
        IList<IDomain> IRaceTypeSystem.PrimaryTypes
        {
            get { throw new NotImplementedException(); }
        }

        IList<IDomainConstructor> IRaceTypeSystem.PrimaryTypeConstructors
        {
            get { throw new NotImplementedException(); }
        }

        bool IRaceTypeSystem.IsDerivedFromPrimaries(IDomain domain)
        {
            throw new NotImplementedException();
        }

        IDomain IRaceTypeSystem.TypeAny
        {
            get { return TypeAny; }
        }

        IDomain IRaceTypeSystem.TypeUnit
        {
            get { return TypeUnit; }
        }

        public bool IsRepresentationOf(ObjectDescription objectDescription, IDomain domain)
        {
            throw new NotImplementedException();
        }

        bool IRaceTypeSystem.IsSubtypeOf(IDomain domain1, IDomain domain2)
        {
            throw new NotImplementedException();
        }

        IDomainSubtypingInfo IRaceTypeSystem.GetSubtypingInfo(IDomain domain)
        {
            throw new NotImplementedException();
        }

        IDomainAttributeCollection IRaceTypeSystem.GetDomainAttributes(IDomain domain)
        {
            throw new NotImplementedException();
        }

        bool IRaceTypeSystem.IsFunction(IDomain domain)
        {
            throw new NotImplementedException();
        }

        IFunctionTypeInfo IRaceTypeSystem.GetFunctionTypeInfo(IDomain domain)
        {
            throw new NotImplementedException();
        }

        bool IRaceTypeSystem.IsComplex(IDomain domain)
        {
            throw new NotImplementedException();
        }

        IDomainStructureInfo IRaceTypeSystem.GetStructureInfo(IDomain domain)
        {
            throw new NotImplementedException();
        }

        bool IRaceTypeSystem.IsCollection(IDomain domain)
        {
            throw new NotImplementedException();
        }

        ICollectionDomain IRaceTypeSystem.GetCollectionInfo(IDomain domain)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Imaplementation Details

        #region Types Roster

        private long _lastUsedId = 0;
        private readonly Dictionary<long,RaceType> _typesById = new Dictionary<long, RaceType>();

        private long GetNewTypeId(string name, string ns, string source)
        {
            return ++_lastUsedId;
        }

        private RaceType AddType(string name, string ns, string source, IList<RaceType> parents, Type binaryRepresentation = null)
        { 
            return AddType(name, ns, source, parents, ObjectDescription.Get(binaryRepresentation));
        }

        private RaceType AddType(string name, string ns, string source, IList<RaceType> parents, ObjectDescription binaryRepresentation)
        {
            //TODO: добавить проверку на существование такого типа
            var id = GetNewTypeId(name, ns, source);
            var type = new RaceType(id, name, ns, source, parents, binaryRepresentation);
            _typesById[id] = type;
            AddTypeSubtypingInfo(type);
            return type;
        }
        #endregion

        #region Subtyping
        private readonly Dictionary<long,List<RaceType>> _supertypes = new Dictionary<long, List<RaceType>>();

        private void AddTypeSubtypingInfo(RaceType type)
        {
            var supertypesList = new List<RaceType>();
            var queue = new Queue<RaceType>();
            foreach (var parent in type.Parents)
                queue.Enqueue(parent);
            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                supertypesList.Add(item);
                foreach (var itemsParent in item.Parents)
                    queue.Enqueue(itemsParent);
            }
            supertypesList.Sort((type1, type2) => type1.GetHashCode().CompareTo(type2.GetHashCode()));

            var id = type.GetHashCode();
            _supertypes[id] = supertypesList;
        }

        private List<RaceType> GetSupertypes(RaceType type)
        {
            return _supertypes[type.GetHashCode()];
        }

        public IList<IDomain> GetAncestors(IDomain domain)
        {
            throw new NotImplementedException();
        }


        #endregion

        #endregion
        private class SubtypingInfoContainer
        {
            private readonly RaceTypeSystem _raceTypeSystem;
            private readonly List<List<long>> list;

            public SubtypingInfoContainer(RaceTypeSystem raceTypeSystem)
            { 
                _raceTypeSystem = raceTypeSystem;
            }
        }
    }
}