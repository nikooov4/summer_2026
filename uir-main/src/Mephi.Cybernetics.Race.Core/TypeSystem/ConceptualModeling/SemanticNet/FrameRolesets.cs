using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mephi.Cybernetics.Race.Core.TypeSystem.ConceptualModeling.SemanticNet
{
    public class RaceCharacteristicRoleset : ISemanticRoleset
    {
        private static RaceCharacteristicRoleset _CharacteristicRoleset;
        public static RaceCharacteristicRoleset CharacteristicRoleset => _CharacteristicRoleset 
            ??= new RaceCharacteristicRoleset();
        private RaceCharacteristicRoleset()
        {
            
        }

        public string Name => throw new NotImplementedException();

        public ReadOnlyCollection<SemanticRoleInfo> Roles => throw new NotImplementedException();

        public RolesetType Type => RolesetType.Characteristic;

    }

    public class RaceFunctionalRoleset : ISemanticRoleset
    {
        public string Name => throw new NotImplementedException();

        public ReadOnlyCollection<SemanticRoleInfo> Roles => throw new NotImplementedException();

        public RolesetType Type => RolesetType.Functional;
    }

    public class RaceEventRoleset : ISemanticRoleset
    {
        public string Name => throw new NotImplementedException();

        public ReadOnlyCollection<SemanticRoleInfo> Roles => throw new NotImplementedException();

        public RolesetType Type => RolesetType.Relationship;
    }

    public class RolesetCollection<TRoleset> : ICollection<TRoleset> where TRoleset : ISemanticRoleset 
    {
        // не понятно, что возвращать для функционального roleset, т.к. там бесконечная коллекция
        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => true;

        public RolesetCollection<TRoleset> WithRole(SemanticRoleInfo role)
        {
            throw new NotImplementedException();
        }

        public void Add(TRoleset item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(TRoleset item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(TRoleset[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TRoleset item)
        {
            throw new NotImplementedException();
        }

        #region Implementation of IEnumerable

        public IEnumerator<TRoleset> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }

    public class RaceEventRolesetCollection : RolesetCollection<RaceEventRoleset>
    {
        
    }


    public class RaceFunctionalRolesetCollection : RolesetCollection<RaceFunctionalRoleset>
    {

    }


    public class SemacticNet
    {
        public ICollection<RaceEventRoleset> EventRolesets => throw new NotImplementedException();
    }
}
