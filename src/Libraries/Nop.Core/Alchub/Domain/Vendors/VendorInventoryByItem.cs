using System.ComponentModel;

namespace Nop.Core.Alchub.Domain.Vendors
{
    /// <summary>
    /// Represents a vendor inventory by item
    /// </summary>
    public class VendorInventoryByItem
    {
        [DisplayName("Scan Code")]
        public string ScanCode{get; set;}

        [DisplayName("Current Quantity")]
        public string CurrentQuantity{get; set;}

        [DisplayName("Cost")]
        public string Cost{get; set;}
    }
}
