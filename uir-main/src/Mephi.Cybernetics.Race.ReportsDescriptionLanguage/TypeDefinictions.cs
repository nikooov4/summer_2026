using System.Collections;
using System.Diagnostics;
using System.Linq;



namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage.TypeSystem
{

    class CSTypeWrapper<T> : IAtomicType
    {
        Type CSType => typeof(T);

        public string? Name => typeof(T).Name;

        public Type? InstanceCSType => typeof(IAtomicTypeInstance);

        public bool CanInstantiate => true;

        public static CSTypeWrapper<T> TypeCSObject {get;} = new CSTypeWrapper<T>();


        public bool IsSubtypeOf(IType type)
        {
            return type is CSTypeWrapper<T> otherWrapper &&
                this.CSType.IsSubclassOf(otherWrapper.CSType);
        
        }

        public IAtomicTypeInstance InstantiateWithCSType(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new WrongArgumentsNumberException();
            }
            T obj = (T) args[0];
            return new CSTypeWrapperInstance<T>(obj);
        }

        public IAtomicTypeInstance Instantiate(List<IInstance> args)
        {
            if (args.Count != 1)
            {
                throw new WrongArgumentsNumberException();
            }
            TypesUtils.ThrowIfNotExpectedType(args[0], this);
            var wrapper = (args[0] as CSTypeWrapperInstance<T>);
            return new CSTypeWrapperInstance<T>(wrapper.Item);
        }

        IInstance IInstantiableType.Instantiate(List<IInstance> args)
        {
            return Instantiate(args);
        }
    }

    class CSTypeWrapperInstance<T> : IAtomicTypeInstance
    {
        public T Item {get;}

        public CSTypeWrapper<T> Type {get;} = CSTypeWrapper<T>.TypeCSObject;

        IInstantiableType IInstance.Type => Type;

        internal CSTypeWrapperInstance(T item)
        {
            this.Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public bool Equals(IInstance? other)
        {
            return other is CSTypeWrapperInstance<T> wrapper &&
                (Item?.Equals(wrapper.Item) ?? wrapper.Item == null);
        }

        public override int GetHashCode()
        {
            return Item?.GetHashCode() ?? 0;
        }
    }

    class OrderedSetType : ImmutableCollectionTypeBase
    {
        public override string Name => "Set";

        public override Type? InstanceCSType => typeof(OrderedSetInstance);
        public override IType ElementType {get;}

        public override bool CanInstantiate => true;

        public OrderedSetType(IType genericArgType)
        {
            ElementType = genericArgType;
        }

        public override IAtomicTypeInstance Instantiate(List<IInstance> args)
        {
            if (args.Count != 1)
            {
                throw new WrongArgumentsNumberException();
            }
            return new OrderedSetInstance(this, args[0]);
        }

        public override IAtomicTypeInstance InstantiateWithCSType(List<object> args)
        {
            //тут должны быть другие исключения, а мб их вообще быть не должно
            if (args.Count != 1)
            {
                throw new WrongArgumentsNumberException();
            }
            var list = args[0] as List<IInstance> ?? throw new TypeMismatchException();
            return new OrderedSetInstance(this, list);
        }
    }

    class OrderedSetInstance : OrderedImmutableCollectionInstanceBase
    {
        private Dictionary<IInstance, int> numberIndexByElement;
        private List<IInstance> elementsOrdered;
        public OrderedSetType SetType {get;}
        // protected override IEnumerable<IInstance> Items => elementsOrdered;
        public override IInstantiableType Type => this.SetType;

        internal OrderedSetInstance(OrderedSetType type, IEnumerable<IInstance> values)
        {
            SetType = type;
            foreach (var value in values)
            {
                TypesUtils.ThrowIfNotExpectedType(value, SetType.ElementType);
            }

            numberIndexByElement = new Dictionary<IInstance, int>();
            elementsOrdered = new List<IInstance>();

            int index = 0;
            foreach (var value in values)
            {
                numberIndexByElement.Add(value, index);
                elementsOrdered.Add(value);
                index++;
            }
        }

        internal OrderedSetInstance(OrderedSetType type, IInstance other)
        {
            SetType = type;
            TypesUtils.ThrowIfNotExpectedType(other, SetType);
            var otherSet = other as OrderedSetInstance ?? throw new UnreachableException();
            numberIndexByElement = new Dictionary<IInstance, int>(otherSet.numberIndexByElement);
            elementsOrdered = new List<IInstance>(otherSet.elementsOrdered);
        }

        public override IEnumerator<IInstance> GetEnumerator()
        {
            return elementsOrdered.GetEnumerator();
        }

        public bool Contais(IInstance instance)
        {
            return numberIndexByElement.ContainsKey(instance);
        }

        public int GetIndexOf(IInstance instance)
        {
            return numberIndexByElement[instance];
        }

        public bool IsSubsetOf(OrderedSetInstance other)
        {
            TypesUtils.ThrowIfNotExpectedType(other, this.Type);
            foreach(var el in this.numberIndexByElement.Keys)
            {
                if(!other.numberIndexByElement.ContainsKey(el))
                {
                    return false;
                }
            }
            return true;
        }
    }

    class AllowedValuesConstr : AtomicTraitBase<AllowedValuesConstr>, IConstrTrait
    {
        public OrderedSetInstance AllowedValuesSet {get;}

        public override Type? TraitInstanceCSType => throw new NotImplementedException();

        public override IInstantiableType ExpectedBaseType => throw new NotImplementedException();

        public AllowedValuesConstr(OrderedSetInstance set)
        {
            AllowedValuesSet = (OrderedSetInstance)set.SetType.InstantiateWithCSType([set]);
        }

        public bool IsConstrSatisfiedWith(IAtomicTypeInstance instance)
        {
            return AllowedValuesSet.Contais(instance);
        }

        public override bool IsSubtraitOf(IAtomicTrait trait)
        {
            return trait is AllowedValuesConstr allowedValuesOther &&
                this.AllowedValuesSet.IsSubsetOf(allowedValuesOther.AllowedValuesSet);
        }

        public override IAtomicTraitInstance InstantiateTrait(List<IInstance> args)
        {
            if (args.Count != 0)
            {
                throw new WrongArgumentsNumberException();
            }
            return 
        }

        public override IAtomicTrait Init(CompositeType compositeType)
        {
            return this;
        }
    }

    class AllowedValuesConstrInstance : AtomicTraitInstanceBase<AllowedValuesConstrInstance, AllowedValuesConstr>
    {
        public override IAtomicTrait Type => AllowedTypeType;
        public AllowedValuesConstr AllowedTypeType {get;}

        public override bool Equals(IAtomicTraitInstance? other)
        {
            return other is IAtomicTraitInstance;
        }
        internal AllowedValuesConstrInstance(AllowedValuesConstr type)
        {
            AllowedTypeType = type;
        }
    }

}   