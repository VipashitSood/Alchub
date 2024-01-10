using System;
using System.Text.RegularExpressions;

namespace Nop.Services.Alchub.Common
{
    public static class AlchubCommonHelper
    {
        /// <summary>
        /// Formate phone number string
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="phoneFormat"></param>
        /// <returns></returns>
        public static string FormatPhoneNumber(string phoneNumber, string phoneFormat)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return string.Empty;

            if (string.IsNullOrEmpty(phoneFormat))
                phoneFormat = "(###)###-####";

            var regex = new Regex(@"[^\d]");
            phoneNumber = regex.Replace(phoneNumber, "");

            if (phoneNumber.Length > 0)
                phoneNumber = Convert.ToInt64(phoneNumber).ToString(phoneFormat);

            return phoneNumber;
        }
    }
}
