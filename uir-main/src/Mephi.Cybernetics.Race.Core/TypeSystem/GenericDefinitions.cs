using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core.TypeSystem
{
    public interface IArgumentInfo<out TDomain>
    {
        int Index { get; }
        string Name { get; }
        /// <summary>
        /// Type of the argument
        /// </summary>
        TDomain Range { get; }
        bool IsMandatory { get; }
        bool IsContraVariant { get; }
        bool IsCovariant { get; }
        bool HasDefaultValue { get; }
        Optional<object> DefaultValue { get; }
        // modifiers, such as: IsMandatory, Is(Co)Variant, etc. ...
    }

    public class SignatureArgumentInfo<T> : IArgumentInfo<T>
    {
        public SignatureArgumentInfo(int index, string name, T range, bool isVariant, bool isCovariant)
        {
            Index = index;
            Name = name;
            Range = range;
            IsContraVariant = isVariant;
            IsCovariant = isCovariant;
            DefaultValue = Optional<object>.None;
        }

        public SignatureArgumentInfo(int index, string name, T range, bool isVariant, bool isCovariant, Optional<object> defaultValue)
        {
            Index = index;
            Name = name;
            Range = range;
            IsContraVariant = isVariant;
            IsCovariant = isCovariant;
            DefaultValue = defaultValue;
        }

        public int Index { get; }

        public string Name { get; }

        public T Range { get; }

        public bool IsMandatory => !DefaultValue.HasValue;

        public bool IsContraVariant { get; }

        public bool IsCovariant { get; }

        public bool HasDefaultValue => DefaultValue.HasValue;

        public Optional<object> DefaultValue { get; }
    }

    public interface ISignature<out TDomain, out TArgumentInfo> //: IEnumerable<TArgumentInfo> where TArgumentInfo : IArgumentInfo<TDomain>
    {
        TArgumentInfo[] Arguments { get; }
        TArgumentInfo this[int index] { get; }
        TArgumentInfo this[string name] { get; }
        int Arity { get; }
    }

    public interface IFunctionSignature<out TDomain, out TArgumentInfo> : ISignature<TDomain, TArgumentInfo> where TArgumentInfo : IArgumentInfo<TDomain>
    {
        TDomain ReturnType { get; }
        //TDomain Call(TArgumentInfo[] args);
    }

    public interface IAttributeBase<out TDomain> : IAttributeBase2<TDomain,TDomain>
        where TDomain : IDomain
    {
    }

    public interface IAttributeBase2<out TDomain, out TRange>
        where TDomain : IDomain
        where TRange : IDomain
    {
        string Name { get; }
        TDomain Domain { get; }
        TRange Range { get; }
        bool IsReadOnly { get; }
        object GetValue(object o);
        void SetValue(object o, object value);
        T GetValue<T>(object o);
        void SetValue<T>(object o, T value);
    }

    //public interface IAttributeCollection<out TDomain, out TAttribute> : IEnumerable<TAttribute> where TAttribute : IAttributeBase<TDomain>
    public interface IAttributeCollection<out TDomain, out TAttribute> : IAttributeCollection2<TDomain,TDomain,TAttribute>
        where TAttribute : IAttributeBase2<TDomain, TDomain>
        where TDomain : IDomain
    {

    }

    public interface IAttributeCollection2<out TDomain, out TRange, out TAttribute> : IEnumerable<TAttribute>
        where TAttribute : IAttributeBase2<TDomain, TRange>
        where TDomain : IDomain
        where TRange : IDomain
    {
        TDomain ReflectedDomain { get; }
        TAttribute this[int index] { get; }
        TAttribute this[string name] { get; }
        bool Contains(string name);
    }

    public interface IConstructor<TDomain> : IFunctionSignature<TDomain, IArgumentInfo<TDomain>>
    {
        string Name { get; }
        TDomain Construct(IList<TDomain> args);
        TDomain Construct(TDomain[] args);
    }


    public interface IStructureInfo<TDomain, out TConstructor> where TConstructor : IConstructor<TDomain>
    {
        /// <summary>
        /// An entity (an individual), whose structure is being reflected
        /// </summary>
        TDomain ReflectedDomain { get; }
        TConstructor Constructor { get; }
        IList<TDomain> Components { get; }
    }

    public interface ISubtypingInfo<TDomain>
    {
        /// <summary>
        /// Parent by definition, not including inferred (direct or indirect) supertypes
        /// </summary>
        IReadOnlyList<TDomain> Parents { get; }
        bool IsSubtypeOf(TDomain domain);
    }
}
