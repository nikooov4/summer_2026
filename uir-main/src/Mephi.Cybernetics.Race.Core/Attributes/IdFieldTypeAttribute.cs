using System;

namespace Mephi.Cybernetics.Race.Core.Attributes
{
	public class IdFieldTypeAttribute : Attribute
	{
        private Type _idType;

		public IdFieldTypeAttribute(Type idType)
        {
            _idType = idType;
        }

        public Type IdType
        {
            get
            {
                return _idType;
            }
        }
	}
}
