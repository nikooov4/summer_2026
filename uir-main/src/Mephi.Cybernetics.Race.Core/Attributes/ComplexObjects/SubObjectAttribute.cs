using System;

namespace Mephi.Cybernetics.Race.Core.Attributes.ComplexObjects
{
    public class SubObjectAttribute : Attribute
    {
        private readonly int _index;

        public SubObjectAttribute()
        {
            _index = -1;
        }

        public SubObjectAttribute(int index)
        {
            _index = index;
        }

        public int Index
        {
            get { return _index; }
        }
    }
}
