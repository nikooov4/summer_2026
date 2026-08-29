using System.Collections;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Omelea.Core;

public interface IService
{
    string Name { get; }

    HashSet<IFeature> Contributions { get; }
    HashSet<IFeature> Requiremets { get; }
}

public interface IFeature
{
    string Name { get; }
}