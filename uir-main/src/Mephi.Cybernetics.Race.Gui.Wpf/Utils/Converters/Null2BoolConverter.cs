using System;
using System.Globalization;
using System.Windows.Data;

namespace Mephi.Cybernetics.Race.Gui.Wpf.Utils.Converters
{
    public sealed class Null2BoolConverter : IValueConverter
    {

        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null) != Invert;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bValue)
            {
                return bValue == Invert ? null : parameter;
            }
            else
                throw new ArgumentException("Boolean value expected");
        }
    }
}