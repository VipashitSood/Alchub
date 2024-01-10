using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.Calendar
{
    public class DateHelper
    {
        public static DateTime Now()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
        }

        public static DateTime CovertStringToDate(string date)
        {
            return DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        }

        public static string CovertToString(DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd HH:mm");
        }

        public static string CovertDateToString(DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd");
        }
    }
}
