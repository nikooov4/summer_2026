using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Mephi.Cybernetics.Race.Gui.Wpf.Utils.Converters
{
    public sealed class EnumDescriptionTypeConverter<T> : EnumConverter//IValueConverter
    {
        //public object Convert(object value, Type targetType,
        //    object parameter, CultureInfo culture)
        //{
        //    if (targetType == typeof(string))
        //    {
        //        if (value != null)
        //        {
        //            FieldInfo fi = value.GetType().GetField(value.ToString());
        //            if (fi != null)
        //            {
        //                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        //                return ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : value.ToString();
        //            }
        //        }

        //        return string.Empty;
        //    }
        //    return string.Empty;
        //}

        //public object ConvertBack(object value, Type targetType,
        //    object parameter, CultureInfo culture)
        //{
        //    if (value == null) return null;
        //    foreach (var one in Enum.GetValues(parameter as Type))
        //    {
        //        FieldInfo fi = one.GetType().GetField(parameter.ToString());
        //        if (fi != null)
        //        {
        //            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
        //            if ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description)))
        //            {
        //                if (value.ToString() == attributes[0].Description)
        //                    return one;
        //            }

        //        }

        //    }
        //    return null;
        //}
        public EnumDescriptionTypeConverter(Type type)
            : base(type)
        {
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());
                    if (fi != null)
                    {
                        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        return ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : value.ToString();
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == value.ToString())
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == value.ToString())
                        return (T)field.GetValue(null);
                }
            }
            return null;
        }
    }
}
