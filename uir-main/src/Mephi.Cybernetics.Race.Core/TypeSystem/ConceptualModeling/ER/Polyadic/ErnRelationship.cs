using System;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core.Attributes;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.ER.Polyadic
{
    public class ErnRelationship : IRelationship
    {
       
        
        private IEntity _ernEntity;
        private IList<IRelationshipArgumentInfo> _relationshipArgumentInfo;
        private Dictionary<String, IRelationshipArgumentInfo> _relationshipArgumentInfoDictionary;

        public ErnRelationship(
            IList<IRelationshipArgumentInfo> relationshipArgumentInfo,
            IEntity ernEntity,
            string name,
            ISemanticRoleset roleset,
            int arity,
            IRelationshipArgumentInfo[] arguments
        )
        {
            _relationshipArgumentInfo = new List<IRelationshipArgumentInfo>(relationshipArgumentInfo);
            _relationshipArgumentInfoDictionary = new Dictionary<string, IRelationshipArgumentInfo>();
            foreach (var relatioshipArgument in relationshipArgumentInfo)
            {
                _relationshipArgumentInfoDictionary.Add(relatioshipArgument.Name, relatioshipArgument);
            }

            _ernEntity = ernEntity; //точно нужно сюда??
            Name = name;
            Roleset = roleset;
            Arity = arity;
            Arguments = arguments;
        }

        public IRelationshipArgumentInfo this[int index] => _relationshipArgumentInfo[index];

        public IRelationshipArgumentInfo this[string name] => _relationshipArgumentInfoDictionary[name];

        public string Name { get; }

        public ISemanticRoleset Roleset { get; }

        public IRelationshipArgumentInfo[] Arguments { get; }

        public int Arity { get; }

        public string Namespace => throw new NotImplementedException();

        public string Source => throw new NotImplementedException();

        public string FullName => throw new NotImplementedException();

        public string FullyQualifiedName => throw new NotImplementedException();
    }
}