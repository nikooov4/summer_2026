using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mephi.Cybernetics.Race.Gui.Wpf.Utils.Converters
{
    public sealed class Null2VisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                parameter = "Collapsed";
            if (parameter is string visibilityMode)
            {
                switch (visibilityMode)
                {
                    case "Collapsed":
                        return Invert != (value != null) ? Visibility.Visible : Visibility.Collapsed;
                    case "Hidden":
                        return Invert != (value != null) ? Visibility.Visible : Visibility.Hidden;
                    default:
                        throw new ArgumentException();
                }
            }
            else
                throw new ArgumentException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                switch (visibility)
                {
                    case Visibility.Collapsed:
                    case Visibility.Hidden:
                        return !Invert;
                    case Visibility.Visible:
                        return !Invert;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
                throw new NotImplementedException();
        }
    }
}