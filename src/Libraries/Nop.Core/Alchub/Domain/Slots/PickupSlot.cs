using System;

namespace Nop.Core.Domain.Slots
{
    /// <summary>
    /// Represents slot entity
    /// </summary>
    public partial class PickupSlot : BaseEntity
    {
        /// <summary>
        /// Gets or sets the start date
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the end date
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Gets or sets the zone
        /// </summary>
        public int ZoneId { get; set; }

        /// <summary>
		/// Gets or sets a value indicating whether is recurring 
		/// </summary>
		public bool IsRecurring { get; set; }

        /// <summary>
		/// Gets or sets a value indicating whether is recurring type
		/// </summary>
        public RecurringType RecurringType { get; set; }

        /// <summary>
        /// Gets or sets the sequence id
        /// </summary>
        public string Sequence_Id { get; set; }

        /// <summary>
        /// Gets or sets the capacity 
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the week days
        /// </summary>
        public string WeekDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is unavailable 
        /// </summary>
        public bool IsUnavailable { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the zone
        /// </summary>
        public virtual Zone Zone { get; set; }

    }
}
