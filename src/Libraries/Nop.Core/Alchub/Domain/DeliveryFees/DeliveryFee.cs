using System;

namespace Nop.Core.Domain.DeliveryFees
{
    /// <summary>
    /// Represents Delivery Fee entity
    /// </summary>
    public partial class DeliveryFee : BaseEntity
    {
        /// <summary>
        /// Gets or sets Vendor Id
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets Delivery Fee Type Id
        /// </summary>
        public int DeliveryFeeTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Fixed Fee
        /// </summary>
        public decimal FixedFee { get; set; }

        /// <summary>
        /// Gets or sets the Dynamic Base Fee
        /// </summary>
        public decimal DynamicBaseFee { get; set; }

        /// <summary>
        /// Gets or sets the Dynamic Base Distance
        /// </summary>
        public decimal DynamicBaseDistance { get; set; }

        /// <summary>
        /// Gets or sets the Dynamic Extra Fee
        /// </summary>
        public decimal DynamicExtraFee { get; set; }

        /// <summary>
        /// Gets or sets the Dynamic Maximum Fee
        /// </summary>
        public decimal DynamicMaximumFee { get; set; }

        /// <summary>
        /// Gets or sets the date and time when entity was created
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time when entity was updated
        /// </summary>
        public DateTime? UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the Delivery Fee type
        /// </summary>
        public DeliveryFeeType DeliveryFeeType
        {
            get => (DeliveryFeeType)DeliveryFeeTypeId;
            set => DeliveryFeeTypeId = (int)value;
        }
    }
}