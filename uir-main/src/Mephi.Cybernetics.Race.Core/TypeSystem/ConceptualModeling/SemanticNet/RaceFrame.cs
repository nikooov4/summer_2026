using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.SemanticNet
{
    public abstract class RaceFrame : IDomain, ISignature<RaceConcept, SignatureArgumentInfo<RaceConcept>>
    {
        public string Name => throw new System.NotImplementedException();

        public string Namespace => throw new System.NotImplementedException();

        public string Source => throw new System.NotImplementedException();

        public string FullName => throw new System.NotImplementedException();

        public string FullyQualifiedName => throw new System.NotImplementedException();

        public RaceFrameArgumentInfo[] Arguments =>
            throw new System.NotImplementedException();

        RaceFrameArgumentInfo this[int index] =>
            throw new System.NotImplementedException();

        RaceFrameArgumentInfo this[string name] =>
            throw new System.NotImplementedException();

        public int Arity => throw new System.NotImplementedException();

        public virtual bool IsValuation(object[] argument)
        {
            throw new NotImplementedException();
        }

        public abstract bool IsValuation(IDictionary<RaceFrameArgumentInfo,object> argument);

        public virtual bool IsValuation(IDictionary<string, object> argument)
        {
            throw new NotImplementedException();
        }

        #region Implementation of ISignature<out RaceConcept,out SignatureArgumentInfo<RaceConcept>>

        SignatureArgumentInfo<RaceConcept>[] ISignature<RaceConcept, SignatureArgumentInfo<RaceConcept>>.Arguments =>
            throw new System.NotImplementedException();

        SignatureArgumentInfo<RaceConcept> ISignature<RaceConcept, SignatureArgumentInfo<RaceConcept>>.this[int index] =>
            throw new System.NotImplementedException();

        SignatureArgumentInfo<RaceConcept> ISignature<RaceConcept, SignatureArgumentInfo<RaceConcept>>.this[string name] =>
            throw new System.NotImplementedException();

        #endregion
    }

    public class RaceFrameArgumentInfo : IArgumentInfo<RaceConcept>
    {
        public RaceFrameArgumentInfo(int index, string name, RaceConcept range, SemanticRoleInfo role) :
            this(index, name, range, role, Optional<int>.None)
        {
        }

        public RaceFrameArgumentInfo(int index, string name, RaceConcept range, SemanticRoleInfo role, Optional<int> cardinality)
        {
            Index = index;
            Name = name;
            Range = range;
            Role = role;
            Cardinality = cardinality;
        }

        public int Index { get; }

        public string Name { get; }

        public RaceConcept Range { get; }

        public SemanticRoleInfo Role { get; }

        public Optional<int> Cardinality { get; }

        public bool IsMandatory => throw new NotImplementedException();

        public bool IsContraVariant => throw new NotImplementedException();

        public bool IsCovariant => throw new NotImplementedException();

        public bool HasDefaultValue => throw new NotImplementedException();

        public Optional<object> DefaultValue => throw new NotImplementedException();
    }

    //TODO: реализовать
    // Варианты создания:
    // 1. На основе класса (все экз класса -- экз концепта)
    // 2. На основе enum
    // 3. Перечислением отдельных элементов данного класса
    // 4. Дизъюнктивная сумма (Either?)
    // 5. На основе простого предиката: C<T,T2>: T - тип экз, T2 - тип атрибута 
    public class RaceSimpleFrame : RaceFrame
    {
        public static RaceSimpleFrame From<T>(SemanticRoleInfo role, Func<T, bool> predicate)
        {
            throw new System.NotImplementedException();
        }

        public static RaceSimpleFrame From<T1, T2>(IEnumerable<SemanticRoleInfo> role, Func<T1, T2, bool> predicate)
        {
            throw new System.NotImplementedException();
        }

        //TODO: размножить до 9-10
        public static RaceSimpleFrame From<T1, T2, T3>(IEnumerable<SemanticRoleInfo> role, Func<T1, T2, T3, bool> predicate)
        {
            throw new System.NotImplementedException();
        }

        public static RaceSimpleFrame From(IEnumerable<(SemanticRoleInfo, string, RaceConcept)> signature, Delegate function)
        {
            throw new System.NotImplementedException();
        }

        
#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public static RaceSimpleFrame From(IEnumerable<(SemanticRoleInfo, string, RaceConcept)> signature, ISet<ITuple> extension)
#else
        public static RaceSimpleFrame From(IEnumerable<(SemanticRoleInfo, string, RaceConcept)> signature, ISet<object[]> extension)
#endif
        
        {
            throw new System.NotImplementedException();
        }


        public override bool IsValuation(IDictionary<RaceFrameArgumentInfo, object> argument)
        {
            throw new NotImplementedException();
        }
    }

    public class RaceComplexFrame : RaceFrame
    {
        public RaceComplexConcept Union(IEnumerable<(SemanticRoleInfo, string, RaceConcept)> signature, IEnumerable<RaceFrameCall> frameCalls)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsValuation(IDictionary<RaceFrameArgumentInfo, object> argument)
        {
            throw new NotImplementedException();
        }
    }


    public class RaceFrameCall
    {
        public RaceFrame CalledFrame => throw new NotImplementedException();

        public object this[RaceFrameArgumentInfo argInfo] => throw new NotImplementedException();
        public object this[SemanticRoleInfo role] => throw new NotImplementedException();
        public object this[string roleName] => throw new NotImplementedException();
        public object this[int argIndex] => throw new NotImplementedException();
        public bool CanCall => throw new NotImplementedException(); // CanEvaluate?

        public IReadOnlyCollection<Quantifier> Quantifiers => throw new NotImplementedException();


        private Dictionary<RaceFrameArgumentInfo,object> ActualParameters => throw new NotImplementedException();

    }


}