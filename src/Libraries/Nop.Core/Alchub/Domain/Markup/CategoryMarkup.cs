using System;

namespace Nop.Core.Domain.Markup
{
    /// <summary>
    /// Represents Zone entity
    /// </summary>
    public partial class CategoryMarkup : BaseEntity
    {     
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets a value VendorId
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets a value VendorId
        /// </summary>
        public decimal Markup { get; set; }

        /// <summary>
        /// Gets or sets the date and time when zone was created
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        /// <summary>
        /// Gets or sets the date and time when zone was created
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }


    }
}
