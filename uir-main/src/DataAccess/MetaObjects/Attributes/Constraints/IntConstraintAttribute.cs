using System;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints
{
    [Serializable]
    public class IntConstraintAttribute : FieldConstraintAttribute
	{
        private readonly int _fieldMinValue;
        private readonly int _fieldMaxValue;

		public IntConstraintAttribute(int fieldMinValue, int fieldMaxValue)
		{
			_fieldMinValue = fieldMinValue;
			_fieldMaxValue = fieldMaxValue;

		}

		public int FieldMinValue
		{
			get
			{
				return _fieldMinValue;
			}
		}

		public int FieldMaxValue
		{
			get
			{
				return _fieldMaxValue;
			}
		}

        public override bool CheckValue(IEntity entity, EntityAttributeDescription property, object value)
        {
            if (!(value is int))
                return false;
            else
                return ((int)value <= _fieldMaxValue) && ((int)value >= _fieldMinValue);
        }

        public override bool CheckValue(EntityBase entity, EntityAttributeDescription property)
        {
            return CheckValue(entity, property, entity[property]);
        }

        public override string ErrorMessage
        {
            get { return string.Format("Значение должно быть не меньше '{0}' и не больше '{1}'", _fieldMinValue, _fieldMaxValue); }
        }
	}
}
