using System;

namespace Nop.Core.Alchub.Domain.Vendors
{
    public partial class VendorTiming : BaseEntity
    {
        /// <summary>
        /// Gets or sets vendor identifier
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets day identifier
        /// </summary>
        public int DayId { get; set; }

        /// <summary>
        /// Gets or sets open time utc
        /// </summary>
        public DateTime? OpenTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets close time utc
        /// </summary>
        public DateTime? CloseTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets day off
        /// </summary>
        public bool DayOff { get; set; }

        /// <summary>
        /// Gets or sets the days
        /// </summary>
        public DayofWeek Days
        {
            get => (DayofWeek)DayId;
            set => DayId = (int)value;
        }
    }
}
