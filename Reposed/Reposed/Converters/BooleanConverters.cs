using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Reposed.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = (bool)value;
            if (result)
                return IsInverted ? Visibility.Collapsed : Visibility.Visible;
            else
                return IsInverted ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
