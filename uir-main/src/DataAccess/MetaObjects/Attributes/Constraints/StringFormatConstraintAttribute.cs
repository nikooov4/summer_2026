using System;
using System.Text.RegularExpressions;

namespace Mephi.Cybernetics.Race.DataAccess.MetaObjects.Attributes.Constraints
{
    public class StringFormatConstraintAttribute : FieldConstraintAttribute
    {
        private Regex _regex;
        private string _errorMessage;

        public StringFormatConstraintAttribute(string regex)
        {
            _regex= new Regex(regex);
        }

        public StringFormatConstraintAttribute(string  regex, string errorMessage)
        {
            _regex = new Regex(regex);
            _errorMessage = errorMessage;
        }

        public Regex Regex
        {
            get { return _regex; }
        }

        public override bool CheckValue(IEntity entity, EntityAttributeDescription property, object value)
        {
            if (!(value is string))
                return false;
            else
                return _regex.IsMatch(value as string);
        }

        public override bool CheckValue(EntityBase entity, EntityAttributeDescription property)
        {
            return CheckValue(entity, property, entity[property]);
        }

        public override string ErrorMessage
        {
            get
            {
                if (string.IsNullOrEmpty(_errorMessage))
                    return string.Format("Строка не соответствует заданному формату: {0}", _regex);
                else
                    return _errorMessage;
            }
        }
    }
}
