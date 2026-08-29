using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mephi.Cybernetics.Race.Core.TypeSystem
{
    public class RaceType : IDomain, IDomainSubtypingInfo
    {
        public static RaceTypeConstructor Define()
        {
            return new RaceTypeConstructor();
        }

        private readonly long _internalId;
        private readonly string _name;
        private readonly string _ns;
        private readonly string _source;
        private readonly string _fullName;
        private readonly string _fullyQualifiedName;
        private readonly ReadOnlyCollection<RaceType> _parents;

        internal RaceType(long internalId, string name, string ns, string source, IList<RaceType> parents, ObjectDescription binaryRepresentation = null)
        {
            _internalId = internalId;
            _name = name;
            _ns = ns;
            _fullName = $"{ns}{(!string.IsNullOrEmpty(ns) ? "." : "")}{name}";
            _source = source;
            _fullyQualifiedName = $"[{_source}::{_fullName}]";
            _parents = new ReadOnlyCollection<RaceType>(parents);
            BinaryRepresentation = binaryRepresentation;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Namespace
        {
            get { return _ns; }
        }

        public string Source
        {
            get { return _source; }
        }

        public string FullName
        {
            get { return _fullName; }
        }

        public string FullyQualifiedName
        {
            get { return _fullyQualifiedName; }
        }

        public ObjectDescription BinaryRepresentation { get; }

        public IList<RaceType> Parents
        {
            get { return _parents; }
        }

        public RaceFunctionType FunctionTypeInfo
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSubtypeOf(RaceType type)
        {
            throw new NotImplementedException();
        }

         #region IDomainSubtypingInfo
        IReadOnlyList<IDomain> ISubtypingInfo<IDomain>.Parents
        {
            get { return _parents; }
        }

        bool ISubtypingInfo<IDomain>.IsSubtypeOf(IDomain domain)
        {
            throw new NotImplementedException();
        }
        #endregion

        public override int GetHashCode()
        {
            return (int)_internalId;
        }

        public override bool Equals(object obj)
        {
            return obj != null &&
                obj.GetType() == typeof(RaceType) &&
                // ReSharper disable once PossibleNullReferenceException
                ((obj as RaceType)._internalId == _internalId);
        }
    }

    //[Relationship]
    public class RaceFunctionType : IFunctionTypeInfo
    {
        private readonly RaceType _reflectedType;

        public RaceFunctionType(RaceType reflectedType)
        {
            _reflectedType = reflectedType;
        }

        public RaceType ReflectedType
        {
            get { return _reflectedType; }
        }

        IArgumentInfo<IDomain>[] Arguments
        {
            get { throw new NotImplementedException(); }
        }


        //[Relation(RelationType.OneToOne, typeof(RaceType), "")]
        public RaceType ReflectedDomain
        {
            get { throw new NotImplementedException(); }
        }

        public RaceFunctionArgumentInfo this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        public RaceFunctionArgumentInfo this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public int Arity
        {
            get { throw new NotImplementedException(); }
        }

        public RaceType ReturnType
        {
            get { throw new NotImplementedException(); }
        }

        public RaceType ReflecteDomain
        {
            get { throw new NotImplementedException(); }
        }

        public Delegate AsDelegate()
        {
            throw new NotImplementedException();
        }

        public T AsDelegate<T>() where T : Delegate
        {
            throw new NotImplementedException();
        }

        public bool IsDelegateConformant(Delegate functionDelegate)
        {
            throw new NotImplementedException();
        }

        public object Apply(object function, IList<object> args)
        {
            throw new NotImplementedException();
        }

        public object Apply(object function, object[] args)
        {
            throw new NotImplementedException();
        }

        #region IFunctionTypeInfo

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

        IDomain IFunctionSignature<IDomain, IArgumentInfo<IDomain>>.ReturnType
        {
            get { throw new NotImplementedException(); }
        }

        IDomain IFunctionTypeInfo.ReflectedDomain
        {
            get { throw new NotImplementedException(); }
        }

        Delegate IFunctionTypeInfo.AsDelegate
        {
            get { throw new NotImplementedException(); }
        }


        bool IFunctionTypeInfo.IsDelegateConformant(Delegate functionDelegate)
        {
            throw new NotImplementedException();
        }


        #endregion

    }

    public class RaceFunctionArgumentInfo : IArgumentInfo<RaceType>
    {
        private readonly int _index;
        private readonly string _name;
        private readonly RaceType _range;

        public RaceFunctionArgumentInfo(string name, RaceType range, int index)
        {
            _name = name;
            _range = range;
            _index = index;
        }

        public int Index
        {
            get { return _index; }
        }

        public string Name
        {
            get { return _name; }
        }

        public RaceType Range
        {
            get { return _range; }
        }

        public bool IsMandatory => throw new NotImplementedException();

        public bool IsContraVariant => throw new NotImplementedException();

        public bool IsCovariant => throw new NotImplementedException();

        public bool HasDefaultValue => throw new NotImplementedException();

        public Optional<object> DefaultValue => throw new NotImplementedException();
    }

}