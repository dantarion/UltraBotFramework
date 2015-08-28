using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UltraBotUI
{
    class Helpers
    {
        public class EnumConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
            {
                if (value.GetType().IsEnum)
                {
                    return value.ToString();
                }
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter,
                                      System.Globalization.CultureInfo culture)
            {
                return value;
            }
        }
    }
}
