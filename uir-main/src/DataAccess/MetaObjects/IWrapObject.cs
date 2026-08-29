using System;
using System.Collections.Generic;
using System.Text;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects
{
	public interface IWrapObject
	{
		[Internal]
        EntityBase WrappedObject
		{
			get;
			set;
		}
	}
}
