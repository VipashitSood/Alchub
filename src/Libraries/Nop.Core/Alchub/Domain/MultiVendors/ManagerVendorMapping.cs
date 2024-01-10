namespace Nop.Core.Domain.MultiVendor
{
    //Represents a manager vendor mapping entity
    public class ManagerVendorMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets a multi vendor identifier
        /// </summary>
        public int MultiVendorId { get; set; }

        /// <summary>
        /// Gets or sets a vendor identifier
        /// </summary>
        public int VendorId { get; set; }
    }
}
