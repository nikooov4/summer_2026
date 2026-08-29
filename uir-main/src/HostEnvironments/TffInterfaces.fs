namespace Mephi.Cybernetics.Race.Core.Host.Definitions

type 
    IFunctionalAbstraction = interface
    inherit ITerm
    inherit ICompositeTerm
        abstract GetBody : ITerm
        abstract GetBasicBody : ITerm
        abstract GetBoundVariables : seq<IVariable>
        abstract GetBoundVariable : IVariable
    end