using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public interface IExtendedEnumerable<T> : IEnumerable<T> where T : IEntity
    {
        T First();
        T First(T defaultValue);

        IExtendedEnumerable<T> Where(string propertyName, object value);

        IExtendedEnumerable<T> Union(IExtendedEnumerable<T> list);
        IExtendedEnumerable<T> Intersection(IExtendedEnumerable<T> list);
        IExtendedEnumerable<T> Except(IExtendedEnumerable<T> list);

        bool Any();
        bool All();

        int Count();

        IExtendedEnumerable<T> Map();
        TPropertyType Fold<TPropertyType>(TPropertyType accumulator);
    }


    public interface IQuery<T> : IExtendedEnumerable<T> where T : IEntity
    {
        
    }

    public interface IExtendedEnumerable : IExtendedEnumerable<IEntity>
    {
        
    }
}
