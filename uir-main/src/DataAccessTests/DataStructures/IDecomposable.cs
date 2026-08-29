using Mephi.Cybernetics.Race.DataAccess;

namespace Mephi.Cybernetics.Race.DataAccessTests.DataStructures
{
    public interface IDecomposable : IEntity
    {
        object[] Parts { get; }
    }
}
