using System;
using System.Collections.ObjectModel;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.Idef0
{
    public class Idef0ProcessInfo : IInformationProcessInfo
    {
        private ReadOnlyCollection<Idef0ProcessParticipantInfo> _inputs;
        private ReadOnlyCollection<Idef0ProcessParticipantInfo> _outputs;
        private ReadOnlyCollection<Idef0ProcessParticipantInfo> _control;
        private ReadOnlyCollection<Idef0ProcessParticipantInfo> _resources;

        public Idef0ProcessInfo(ObjectDescription objectDescription)
        {
            
        }

        public ReadOnlyCollection<Idef0ProcessParticipantInfo> Inputs
        {
            get { return _inputs; }
        }

        public ReadOnlyCollection<Idef0ProcessParticipantInfo> Outputs
        {
            get { return _outputs; }
        }

        public ReadOnlyCollection<Idef0ProcessParticipantInfo> Control
        {
            get { return _control; }
        }

        public ReadOnlyCollection<Idef0ProcessParticipantInfo> Resources
        {
            get { return _resources; }
        }


        ReadOnlyCollection<IProcessArgumentInfo<IEntity>> IProcessInfo<IEntity>.Inputs
        {
            get { throw new NotImplementedException(); }
        }

        ReadOnlyCollection<IProcessArgumentInfo<IEntity>> IProcessInfo<IEntity>.Outputs
        {
            get { throw new NotImplementedException(); }
        }
    }
}