using System.Collections.Generic;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccess
{
	public enum WrapObjectState
	{
		Unchanged,
		Changed,
		New,
		Deleted
	}

	public interface IWrapObject
	{
		object ID
		{
			get;
			set;
		}

		[Internal]
		WrapObjectState State
		{
			get;
			set;
		}

        EntityBase WrappedObject { get; set; }
    }
}
