using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows;

namespace BencinaChile
{
    public class StationVisitiblityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DesignerProperties.IsInDesignTool)
                return Visibility.Visible;
            return (value != null) ? Visibility.Visible : Visibility.Collapsed;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
