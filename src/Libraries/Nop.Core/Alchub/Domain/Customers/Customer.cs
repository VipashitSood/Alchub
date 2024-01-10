namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a extended customer
    /// </summary>
    public partial class Customer
    {
        /// <summary>
        /// Gets or sets the last searched geo coordinates(latitude & longitude) by the customer.
        /// </summary>
        public string LastSearchedCoordinates { get; set; }
        
        /// <summary>
        /// Gets or sets the last searched text(area/address) by the customer.
        /// </summary>
        public string LastSearchedText { get; set; }

        /// <summary>
        /// Gets or sets search from favorite store toggle.
        /// </summary>
        public bool IsFavoriteToggleOn { get; set; }
    }
}