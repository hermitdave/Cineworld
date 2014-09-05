using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if WINDOWS_PHONE
using System.Windows.Data;
#else
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
#endif

namespace Cineworld
{
    public class VisibilityConverter : IValueConverter
    {
        public enum Mode
        {
            Default,
            Inverted,
        }

#if WINDOWS_PHONE
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            Visibility returnVisibility = Visibility.Visible;
            Mode mode = Mode.Default;
            
            if (parameter != null)
            {
                try
                {
                    mode = (Mode)Enum.Parse(typeof(Mode), (string)parameter, true);
                }
                catch
                {
                    mode = Mode.Default;
                }
            }

            if (value == null)
            {
                returnVisibility = Visibility.Collapsed;
            }
            else if(value is bool)
            {
                bool bVal = (bool)value;
                if (!bVal)
                    returnVisibility = Visibility.Collapsed;
            }
            else if (value is string)
            {
                string itemVal = value as String;

                if (String.IsNullOrWhiteSpace(itemVal))
                    returnVisibility = Visibility.Collapsed;
            }
            else if (value is IList)
            {
                IList objectList = value as IList;
                if (objectList == null || objectList.Count == 0)
                    returnVisibility = Visibility.Collapsed;    
            }

            if (mode == Mode.Inverted)
                return returnVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            else
                return returnVisibility;
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
