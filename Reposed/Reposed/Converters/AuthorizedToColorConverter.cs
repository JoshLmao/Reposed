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
    public class AuthorizedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isAuthorized = (bool)value;

            string colorId = isAuthorized ? "PrimaryTextBrush" : "ErrorTextBrush";
            return Application.Current.FindResource(colorId);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
