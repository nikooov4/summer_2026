using System;

namespace Mephi.Cybernetics.Race.Core.UI.Attributes
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct|AttributeTargets.Interface,AllowMultiple = true)]
    public class PresenterAttribute : Attribute
    {
        public PresenterAttribute(Type objectType) : this("", objectType)
        {
        }

        public PresenterAttribute(string applicationArea, Type modelViewType)
        {
            ApplicationArea = applicationArea;
            ModelViewType = modelViewType;
        }

        public string ApplicationArea { get; }
        public Type ModelViewType { get; }
    }
}