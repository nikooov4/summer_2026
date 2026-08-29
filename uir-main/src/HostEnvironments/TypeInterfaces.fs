namespace Mephi.Cybernetics.Race.HostEnvironments

type
    ITypeSystem = interface
        abstract GetContext : ITypeContext
    end

and
    ITyped = interface
        abstract GetType : ITyped
    end

and
    ITypeContext = interface
        abstract Dom : seq<ITyped>
        abstract Include : ITyped -> bool
    end
