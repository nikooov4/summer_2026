namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.Idef0
{
    //TODO: заменить IEntity на EntityDescription
    public class ProcessParticipantInfo : IProcessArgumentInfo<IEntity>
    {
        private string _name;
        private int _index;
        private IEntity _range;
        protected SemanticRoleInfo _semanticRoleInfo;

        public ProcessParticipantInfo()
        {

        }

        public int Index
        {
            get { return _index; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IEntity Range
        {
            get { return _range; }
        }

        IEntity IArgumentInfo<IEntity>.Range
        {
            get { return _range; }
        }

        SemanticRoleInfo IProcessArgumentInfo<IEntity>.RoleInfo
        {
            get { return _semanticRoleInfo; }
        }

        public bool IsMandatory => throw new System.NotImplementedException();

        public bool IsContraVariant => throw new System.NotImplementedException();

        public bool IsCovariant => throw new System.NotImplementedException();

        public bool HasDefaultValue => throw new System.NotImplementedException();

        public Optional<object> DefaultValue => throw new System.NotImplementedException();
    }

    public class Idef0ProcessParticipantInfo : ProcessParticipantInfo
    {
        private readonly Idef0SemanticRoles _role;

        public Idef0ProcessParticipantInfo(Idef0SemanticRoles role)
        {
            _role = role;
        }

        public Idef0SemanticRoles Idef0Role
        {
            get { return _role; }
        }
    }
}