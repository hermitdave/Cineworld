using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using System.Windows.Data;
#else
using Windows.UI.Xaml.Data;
#endif

namespace Cineworld
{
    public class DateTimeToStringConverter : IValueConverter
    {
        const string DateFormat = "ddd, dd MMM yyy";
        public DateTimeToStringConverter() { }

#if WINDOWS_PHONE
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            if (value == null)
                return String.Empty;
            
            if (value is DateTime)
            {
                return ConvertData((DateTime)value);
            }

            return null;
        }

        public static string ConvertData(DateTime value)
        {
                DateTime dt = (DateTime)value;
                if (dt.Date == DateTime.UtcNow.Date)
                    return String.Format("{0} - today", value.ToString(DateFormat));
                else if (dt.Date == DateTime.UtcNow.Date.AddDays(1))
                    return String.Format("{0} - tomorrow", value.ToString(DateFormat));
                else
                    return dt.ToString(DateFormat);
        }

#if WINDOWS_PHONE
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#endif
        {
            throw new NotImplementedException();
        }
    }
}
