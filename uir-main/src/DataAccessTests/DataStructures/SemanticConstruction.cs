using Mephi.Cybernetics.Race.DataAccess;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataStructures
{
    public class SemanticConstruction : EntityBase, IDecomposable
    {
        public string Name
        {
            get { return GetValue<string>("Name"); }
            set { SetValue("Name", value); }
        }

        public SemanticConstruction Parent
        {
            get { return GetValue<SemanticConstruction>("Parent"); }
            set { SetValue("Parent", value); }
        }

        public virtual object[] Parts
        {
            get { return null; }
        }
    }
}
