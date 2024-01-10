namespace Nop.Web.Models.ShoppingCart
{
    public partial record SlotFeeModel
    {

        /// <summary>
        /// Gets or sets SlotName
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Gets or sets SlotFee
        /// </summary>
        public string SlotFee { get; set; }

        public string SlotDateTime { get; set; }
    }
}