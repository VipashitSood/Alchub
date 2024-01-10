namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order average report line
    /// </summary>
    public partial class OrderAverageReportLine
    {
        /// <summary>
        /// Gets or sets order service fee
        /// </summary>
        public decimal OrderServiceFee { get; set; }

        public decimal OrderSlotFee { get; set; }
    }
}
