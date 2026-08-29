using System;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.SemanticNet
{
    public interface IVariable
    {
        string Name { get; }
    }
    public interface ITypedVariable<out TType> : IVariable where TType : IDomain
    {
        TType Type { get; }
    }

    public class Quantifier
    {
        public static Quantifier ForAll(ITypedVariable<RaceConcept> variable) => new Quantifier(variable,true);
        public static Quantifier Exists(ITypedVariable<RaceConcept> variable) => new Quantifier(variable,false);

        protected Quantifier(ITypedVariable<RaceConcept> variable, bool isUniversal)
        {
            Variable = variable;
            IsExistential = !isUniversal;
            IsUniversal = isUniversal;
        }

        public bool IsExistential { get; }
        public bool IsUniversal { get; }

        public ITypedVariable<RaceConcept> Variable { get; }

        // public Interval<?> Cardinality { get;}
    }

}