using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mephi.Cybernetics.Race.Gui.Wpf.Utils.Converters
{
    public sealed class Bool2VisibilityConverter : IValueConverter
    {

        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string visibilityMode && value is bool isVisible)
            {
                switch (visibilityMode)
                {
                    case "Collapsed":
                        return Invert != isVisible ? Visibility.Visible : Visibility.Collapsed;
                    case "Hidden":
                        return Invert != isVisible ? Visibility.Visible : Visibility.Hidden;
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