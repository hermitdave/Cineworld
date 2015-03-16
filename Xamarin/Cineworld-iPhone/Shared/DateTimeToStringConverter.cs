using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cineworld
{
    public class DateTimeToStringConverter
    {
        const string DateFormat = "ddd, dd MMM yyy";
        public DateTimeToStringConverter() { }

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
    }
}
