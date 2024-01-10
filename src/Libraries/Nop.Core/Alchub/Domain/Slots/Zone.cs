using System;

namespace Nop.Core.Domain.Slots
{
    /// <summary>
    /// Represents Zone entity
    /// </summary>
    public partial class Zone : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value VendorId
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when zone was created
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether zone is active
        /// </summary>
        public bool IsActive { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether zone IsPickUp
        /// </summary>
        public bool IsPickUp { get; set; }


        /// <summary>
        /// Gets or sets the date and time when zone was created
        /// </summary>
        public int CreatedBy { get; set; }
    }
}
