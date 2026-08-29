using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public interface ICache<IdType, ValueType> where ValueType : IEntity
	{
		ValueType this[IdType key]
		{
			get;
			set;
		}
		int Count
		{
			get;
		}
		bool Contains(IdType id);
		void Add(IdType id, ValueType item);
        List<ValueType> GetAll();
		IdType[] Keys
		{
			get;
		}
		ValueType[] Values
		{
			get;
		}
		IEnumerator<ValueType> GetEnumerator();
	}
}
