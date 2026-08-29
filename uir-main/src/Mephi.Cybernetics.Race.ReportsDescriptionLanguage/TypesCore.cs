using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage.TypeSystem
{

    interface IType
    {
        string? Name {get;}
        bool IsSubtypeOf(IType type);
        // bool AreEqual(IType type);
    }

    interface IInstantiableType : IType
    {
        IInstance Instantiate(List<IInstance> args);
        Type? InstanceCSType {get;}
        bool CanInstantiate {get;}
        // bool CanInstantiateWithCSTypes {get;}
    }

    interface IInstance : IEquatable<IInstance>
    {
        IInstantiableType Type {get;}
        
    }

    interface IAtomicTraitInstance : IEquatable<IAtomicTraitInstance>
    {
        IAtomicTrait Type {get;}
    }


    interface IAtomicTrait
    {
        string Name {get;}
        bool IsSubtraitOf(IAtomicTrait trait);
        //аналогично, нам нужна причина
        bool CompositeTypeValidation(CompositeType compositeType);
        // bool CompositeTraitValidation (CompositeTrait trait);
        //this is wrong, but i dont care rn
        string AtomicTraitTypeIdentifier {get;}
        // IReadOnlyDictionary<string, IType> Args {get;}
        Type? TraitInstanceCSType {get;}
        IAtomicTraitInstance InstantiateTrait();
        bool CanInstantiateTrait {get;}
        IAtomicTrait Init(CompositeType compositeType);
        IInstantiableType ExpectedBaseType {get;}
    }

    
    interface IConstrTrait : IAtomicTrait
    {
        //reason IsAssignableFrom(object obj);
        bool IsConstrSatisfiedWith(IAtomicTypeInstance instance);
        
    }


    interface ICollectionType : IAtomicType
    {
        IType ElementType {get;}
    }

    abstract class ImmutableCollectionTypeBase : ICollectionType
    {
        public abstract IType ElementType {get;}

        public abstract string Name {get;}

        public abstract Type? InstanceCSType { get; }
        public virtual bool CanInstantiate => true;

        public abstract IAtomicTypeInstance Instantiate(List<IInstance> args);
        public abstract IAtomicTypeInstance InstantiateWithCSType(List<object> args);

        public bool IsSubtypeOf(IType type)
        {
            return type is ICollectionType collectionType && 
                type.GetType().IsSubclassOf(this.GetType()) &&
                ElementType.IsSubtypeOf(collectionType.ElementType);
        }

        IInstance IInstantiableType.Instantiate(List<IInstance> args)
        {
            return Instantiate(args);
        }
    }

    abstract class OrderedImmutableCollectionInstanceBase : IAtomicTypeInstance, IEnumerable<IInstance>
    {
        public abstract IInstantiableType Type {get;}

        // protected abstract IEnumerable<IInstance> Items {get;}

        public bool Equals(IInstance? other)
        {
            return other is OrderedImmutableCollectionInstanceBase otherCollection &&
                this.Count() == otherCollection.Count() &&
                this.Zip(otherCollection).All(x => x.First.Equals(x.Second));    
        }

        public abstract IEnumerator<IInstance> GetEnumerator();

        public override int GetHashCode()
        {
            var hashes = new HashCode();
            foreach (var item in this)
            {
                hashes.Add(item.GetHashCode());
            }
            return hashes.ToHashCode();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    interface IAtomicTypeInstance : IInstance
    {

        
    }
    interface IAtomicType : IInstantiableType
    {
        IAtomicTypeInstance InstantiateWithCSType(List<object> args);
        new IAtomicTypeInstance Instantiate(List<IInstance> args);
    }

    class AnyType : IInstantiableType
    {
        public string Name => "Any";
        public bool IsSubtypeOf(IType type)
        {
            return true;
        }
        public bool CanInstantiate => false;

        public Type? InstanceCSType => null;

        public bool CanInstantiateWithCSTypes => false;

        public IInstance Instantiate(List<IInstance> args) => throw new NotSupportedException("Cannot instantiate any type");

        public IInstance InstantiateWithCSTypes(List<object> args)
        {
            throw new NotSupportedException("Cannot instantiate any type");
        }
    }

    //LMAO зачем я это сделал? 
    abstract class AtomicTraitBaseInterface<ThisTrait, TraitGreatestParent> : IAtomicTrait
        where TraitGreatestParent : IAtomicTrait
        where ThisTrait      :  AtomicTraitBaseInterface<ThisTrait, TraitGreatestParent>
    {
        private static string _atomicTraitTypeIdentifierStatic = typeof(TraitGreatestParent).Name;
        public string AtomicTraitTypeIdentifier => _atomicTraitTypeIdentifierStatic;
        public virtual string Name => typeof(ThisTrait).Name;

        public virtual Type? TraitInstanceCSType => null;

        public abstract IInstantiableType ExpectedBaseType {get;}

        // public virtual bool CompositeTraitValidation(CompositeTrait trait) => true;
        public virtual bool CompositeTypeValidation(CompositeType type) =>  ExpectedBaseType == null || type.BaseType == ExpectedBaseType;

        public virtual bool IsSubtraitOf(IAtomicTrait trait)
        {
                return this.GetType().IsSubclassOf(trait.GetType());
        }

        public static bool TryGetFrom(CompositeType compositeType, out ThisTrait? trait)
        {
            trait = null;
            if (compositeType.TryGetTrait(_atomicTraitTypeIdentifierStatic, out var traitInterface))
            {
                if (traitInterface is ThisTrait thisTrait)
                {
                    trait = thisTrait;
                    return true;
                }
            
            }
            return false;
        }

        public static ThisTrait GetFrom(CompositeType compositeType)
        {
            if (TryGetFrom(compositeType, out var trait))
            {
                return trait!;
            }
            throw new ArgumentException($"This composite type doesn't contain trait represented by {typeof(ThisTrait).Name} class");
        }
    

        public virtual IAtomicTraitInstance InstantiateTrait()
        {
            throw new NotSupportedException("Cannot instantiate interface trait");
        }

        public virtual bool CanInstantiateTrait => false;
        public abstract IAtomicTrait Init(CompositeType compositeType);
    }

    abstract class AtomicTraitBase<ThisTrait, TraitGreatestParent> : AtomicTraitBaseInterface<ThisTrait, TraitGreatestParent>
        where TraitGreatestParent : IAtomicTrait
        where ThisTrait      :  AtomicTraitBase<ThisTrait, TraitGreatestParent>
    {
        public abstract override IAtomicTraitInstance InstantiateTrait();
        public abstract override Type? TraitInstanceCSType {get;}
        public override bool CanInstantiateTrait => true;
    }

    abstract class AtomicTraitBase<ThisTrait> : AtomicTraitBase<ThisTrait, ThisTrait> where ThisTrait :  AtomicTraitBase<ThisTrait>
    {}

    abstract class AtomicTraitBaseInterface<ThisTrait> : AtomicTraitBaseInterface<ThisTrait, ThisTrait> where ThisTrait :  AtomicTraitBase<ThisTrait>
    {}

    abstract class AtomicTraitInstanceBase<TraitInstance, TraitTypeGreatestParent> : IAtomicTraitInstance
        where TraitTypeGreatestParent : IAtomicTrait
        where TraitInstance : AtomicTraitInstanceBase<TraitInstance, TraitTypeGreatestParent>
    {
        public abstract IAtomicTrait Type { get; }
        public abstract bool Equals(IAtomicTraitInstance? other);
        private static string _atomicTraitTypeIdentifierStatic = typeof(TraitTypeGreatestParent).Name;


        public static bool TryGetFrom(CompositeTypeInstance compositeInstance, out TraitInstance? trait)
        {
            trait = null;
            if (compositeInstance.TryGetTrait(_atomicTraitTypeIdentifierStatic, out var traitInterface))
            {
                if (traitInterface is TraitInstance thisTrait)
                {
                    trait = thisTrait;
                    return true;
                }
            
            }
            return false;
        }

        public static TraitInstance GetFrom(CompositeTypeInstance compositeInstance)
        {
            if (TryGetFrom(compositeInstance, out var trait))
            {
                return trait!;
            }
            throw new ArgumentException($"This composite type instance doesn't contain trait instance represented by {typeof(TraitInstance).Name}class {typeof(TraitInstance).Name}");
        }
    }
    

    class CompositeType : IInstantiableType
    {
        public IAtomicType BaseType {get;}
        public IReadOnlyDictionary<string, IAtomicTrait> TraitsById => _traitsById;
        public IReadOnlyCollection<IConstrTrait> Constraints => _constrTraitsById.Values;
        private Dictionary<string, IAtomicTrait> _traitsById {get;}
        private Dictionary<string, IConstrTrait> _constrTraitsById {get;}
        public string Name => throw new NotImplementedException();
        public Type InstanceCSType => typeof(CompositeTypeInstance);
        public bool CanInstantiate => BaseType.CanInstantiate && TraitsById.Values.All(x => x.CanInstantiateTrait);
        public bool CanInstantiateWithCSTypes => false;

        public CompositeType(IInstantiableType baseType, IEnumerable<IAtomicTrait> traits)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }
            if (traits == null)
            {
                throw new ArgumentNullException(nameof(traits));
            }
            
            try 
            {
                if (baseType is CompositeType compositeBaseType)
                {
                    this.BaseType = compositeBaseType.BaseType;
                    this._traitsById = compositeBaseType.TraitsById.Values.Concat(traits).ToDictionary(x => x.AtomicTraitTypeIdentifier);
                }
                if (baseType is IAtomicType atomicBaseType)
                {
                    this.BaseType = atomicBaseType;
                    this._traitsById = traits.ToDictionary(x => x.AtomicTraitTypeIdentifier);
                }
                else
                {
                    throw new UnreachableException($"Expected base type of {nameof(CompositeType)} or {nameof(IAtomicType)}, got {nameof(baseType.GetType)}");
                }
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Cannot have multiple traits with same type identifier", ex);
            }
            _constrTraitsById = _traitsById.Values.OfType<IConstrTrait>().ToDictionary(x => x.AtomicTraitTypeIdentifier);
            
            Validate();
            Init();
        }

        private void Validate()
        {
            bool valid = true;
            foreach(var trait in TraitsById.Values)
            {
                valid &= trait.CompositeTypeValidation(this);
            }
            if (!valid)
            {
                throw new ArgumentException("Some trait cannot be applied to this type");
            } 
        }

        private void Init()
        {
            foreach(var trait in TraitsById.Values)
            {
                trait.Init(this);
            }
        }

        public bool IsSubtypeOf(CompositeType supertype)
        {
            if (!this.BaseType.IsSubtypeOf(supertype.BaseType))
            {
                return false;
            }
            foreach(var subtrait in this.TraitsById.Values)
            {
                if (!supertype.TryGetTrait(subtrait.AtomicTraitTypeIdentifier, out var supertrait))
                {
                    return false;
                }
                if (!subtrait.IsSubtraitOf(supertrait!))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsSubtypeOf(IType type)
        {
            if (type is CompositeType supertype)
            {
                return IsSubtypeOf(supertype);
            }
            if (type is IAtomicType atomicBaseSupertype)
            {
                return BaseType.IsSubtypeOf(atomicBaseSupertype);
            }
            if (type is IAtomicTrait supertrait && this.TryGetTrait(supertrait.AtomicTraitTypeIdentifier, out var atomicTrait))
            {
                return atomicTrait!.IsSubtraitOf(supertrait);
            }
            return false;
        }

        public bool TryGetTrait(string traitTypeIdentifier, out IAtomicTrait? trait)
        {
            return TraitsById.TryGetValue(traitTypeIdentifier, out trait);
        }

        public IAtomicTrait GetTrait(string traitTypeIdentifier)
        {
            if (TryGetTrait(traitTypeIdentifier, out var trait))
            {
                return trait!;
            }
            throw new ArgumentException($"Cannot find trait with type identifier {traitTypeIdentifier}", traitTypeIdentifier);
        }

        public bool IsAssignableFrom(IAtomicTypeInstance instance)
        {
            return instance.Type.IsSubtypeOf(this) && 
                _constrTraitsById.Values.All(constr => constr.IsConstrSatisfiedWith(instance));
        }

        public IInstance Instantiate(List<IInstance> args)
        {
            var instances = _traitsById.Values
                .Select(trait => trait.InstantiateTrait())
                .ToDictionary(x => x.Type.AtomicTraitTypeIdentifier);

            return new CompositeTypeInstance(this, BaseType.Instantiate(args), instances);
        }

        public override string ToString()
        {
            return BaseType.ToString() + " " + TraitsById.Values.Aggregate("", (c, x) => c + x.ToString());
        }

        public IInstance InstantiateWithCSTypes(List<object> args)
        {
            throw new NotSupportedException();
        }
    }

    class CompositeTypeInstance : IInstance
    {
        public CompositeType Type {get;}
        private IAtomicTypeInstance _base;
        public IAtomicTypeInstance Base 
        {
            get => _base; 
            set
            {
                if (!IsBaseAssignableFrom(value))
                {
                    throw new ArgumentException($"Cannot assign value to {nameof(Base)}: value violates some constraints");
                }
                _base = value;
            }
        }
        private Dictionary<string, IAtomicTraitInstance> _traitInstancesById {get;}

        IInstantiableType IInstance.Type => Type;

        internal CompositeTypeInstance(CompositeType type, IAtomicTypeInstance baseTypeInstance, Dictionary<string, IAtomicTraitInstance> traitInstances)
        {
            Type = type;
            Base = baseTypeInstance;
            _traitInstancesById = new (traitInstances);
        }

        bool IsBaseAssignableFrom(IAtomicTypeInstance baseInstance)
        {
            return baseInstance.Type.IsSubtypeOf(this.Type.BaseType) && Type.Constraints.All(x => x.IsConstrSatisfiedWith(baseInstance));
        }

        public bool Equals(IInstance? other)
        {
            if (other is not CompositeTypeInstance otherInstance)
            {
                return false;
            }
            if (!Base.Equals(otherInstance.Base))
            {
                return false;
            }
            foreach (var thisTraitInstance in _traitInstancesById.Values)
            {
                if (!otherInstance._traitInstancesById.TryGetValue(thisTraitInstance.Type.AtomicTraitTypeIdentifier, out var otherTraitInstance))
                {
                    return false;
                }
                if (!thisTraitInstance.Equals(otherTraitInstance))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Base);
            foreach (var instance in _traitInstancesById.Values)
            {
                hash.Add(instance.GetHashCode());
            }
            return hash.ToHashCode();
        }

        public bool TryGetTrait(string traitTypeIdentifier, out IAtomicTraitInstance? trait)
        {
            return _traitInstancesById.TryGetValue(traitTypeIdentifier, out trait);
        }

        public IAtomicTraitInstance GetTrait(string traitTypeIdentifier)
        {
            if (TryGetTrait(traitTypeIdentifier, out var trait))
            {
                return trait!;
            }
            throw new ArgumentException($"no trait instance with trait type identifier {traitTypeIdentifier}", traitTypeIdentifier);
        }
    }

    class TypingException : Exception
    {
        public TypingException() {}
        public TypingException(string message) : base(message) {}
        public TypingException(string message, Exception innerException) : base(message, innerException) {}

    }

    class TypeMismatchException : TypingException
    {
        public TypeMismatchException() : base("Type mismatch") {}
        public TypeMismatchException(IType actual, IType expected) : base($"Type mismatch: got {actual}, expected {expected}") {}
        public TypeMismatchException(string message) : base(message) {}
    }

    class WrongArgumentsNumberException : TypingException
    {
        public WrongArgumentsNumberException() : base("Wrong number of arguments") {}
    }

    static class TypesUtils
    {
        public static void ThrowIfNotExpectedType(IType actual, IType expected)
        {
            if (!actual.IsSubtypeOf(expected))
            {
                throw new TypeMismatchException(actual, expected);
            }
        }

        public static void ThrowIfNotExpectedType(IInstance actual, IType expected)
        {
            if (!actual.Type.IsSubtypeOf(expected))
            {
                throw new TypeMismatchException(actual.Type, expected);
            }
        }
    } 

}