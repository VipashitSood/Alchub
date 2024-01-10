using System;

namespace Nop.Services.Alchub.Slots
{
    /// <summary>
    /// Represents the product helper
    /// </summary>
    public partial class SlotHelper
    {
        #region Methods

        /// <summary>
        /// Convert original slot time to 12 hour slot time. 
        /// </summary>
        /// <param name="slotOriginalTime"></param>
        /// <returns></returns>
        public static string ConvertTo12hoursSlotTime(string slotOriginalTime)
        {
            if (string.IsNullOrEmpty(slotOriginalTime))
                return string.Empty;

            var hourse = slotOriginalTime.Split("-");
            if (hourse.Length == 2)
            {
                DateTime dtStart;
                DateTime dtEnd;
                var endHours = hourse[1].Equals("24:00") ? "00:00" : hourse[1]; //handel 12am
                if (DateTime.TryParse(hourse[0], out dtStart) && DateTime.TryParse(endHours, out dtEnd))
                {
                    var start = string.Format("{0:htt}", dtStart).ToLowerInvariant();
                    var end = string.Format("{0:htt}", dtEnd).ToLowerInvariant();
                    return $"{start}-{end}";
                }
            }

            //here means, input string is in wrong formate, then return same in result
            return slotOriginalTime;
        }

        #endregion
    }
}
