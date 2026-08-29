using System;
using System.Collections.ObjectModel;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling
{
    public interface IInformationProcess : IProcess<IEntity> 
    {
    }

    public interface IComputationalProcess : IProcess<IDomain>
    {
    }
    
    public interface IProcess<out TDomain> : IDomain, IFunctionSignature<TDomain, IProcessArgumentInfo<TDomain>> where TDomain : IDomain
    {
        ISemanticRoleset Roleset { get; }
    }


//    public interface IInformationProcessArgumentInfo : IProcessArgumentInfo<IEntity>
//    {
//    }


//    public interface IComputationalProcessArgumentInfo : IProcessArgumentInfo<IDomain>
//    {
//    }

    public interface IProcessArgumentInfo<out TDomain> : IArgumentInfo<TDomain>
    {
        SemanticRoleInfo RoleInfo { get; }
    }

    public class SemanticRoleInfo
    {
        private readonly string _name;
        private readonly ISemanticRoleset _roleset;

        private SemanticRoleInfo(string name, ISemanticRoleset roleset)
        {
            _name = name;
            _roleset = roleset;
        }

        public string Name { get { return _name; } }
        public ISemanticRoleset Roleset { get { return _roleset; } }

    }

    public interface ISemanticRoleset
    {
        string Name { get; }
        ReadOnlyCollection<SemanticRoleInfo> Roles { get; }
        RolesetType Type { get; }
    }

    public enum RolesetType
    {
        Characteristic,
        Functional,
        Relationship
    }


    public interface IInformationProcessInfo : IProcessInfo<IEntity>
    {
    }

    public interface IProcessInfo<TDomain>
    {
        ReadOnlyCollection<IProcessArgumentInfo<TDomain>> Inputs { get; }
        ReadOnlyCollection<IProcessArgumentInfo<TDomain>> Outputs { get; }
    }

    public class RolesetAttribute : Attribute
    {
        private readonly Type _type;

        public RolesetAttribute(Type type)
        {
            _type = type;
            throw new NotImplementedException();
        }

        public Type ProcessType
        {
            get { return _type; }
        }
    }

   
}
