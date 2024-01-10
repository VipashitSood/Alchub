using Nop.Core.Configuration;

namespace Nop.Core.Domain.Settings
{
    /// <summary>
    /// Service fee settings
    /// </summary>
    public class ServiceFeeSettings : ISettings
    {
        /// <summary>
        /// Gets or sets service fee type id
        /// </summary>
        public int ServiceFeeTypeId { get; set; }

        /// <summary>
        /// Gets or sets service fee
        /// </summary>
        public decimal ServiceFee { get; set; }

        /// <summary>
        /// Gets or sets service fee percentage
        /// </summary>
        public decimal ServiceFeePercentage { get; set; }

        /// <summary>
        /// Gets or sets maximum service fee 
        /// </summary>
        public decimal MaximumServiceFee { get; set; }
    }
}
