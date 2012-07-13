using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace BencinaChile
{
    public class StationIdImageConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DesignerProperties.IsInDesignTool)
                return "/Icons/5.png";
            return String.Format("/Icons/{0}.png", value);
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
