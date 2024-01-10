using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.TipFees;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.ShoppingCart
{
    /// <summary>
    /// Represents a order total model
    /// </summary>
    public partial record OrderTotalsModel : BaseNopModel
    {
        public OrderTotalsModel()
        {
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            SlotFeesList = new List<SlotFeeModel>();
            VendorWiseDeliveryFees = new List<VendorWiseDeliveryFee>();
            VendorWiseTipFees = new List<VendorWiseTipFee>();
            AvailableTipTypes = new List<SelectListItem>();
        }

        /// <summary>
        /// Gets or sets service fee
        /// </summary>
        public string ServiceFee { get; set; }

        public string SlotFee { get; set; }

        public IList<SlotFeeModel> SlotFeesList { get; set; }

        /// <summary>
        /// Gets or sets delivery fee
        /// </summary>
        public string DeliveryFee { get; set; }

        /// <summary>
        /// Gets or sets vendor wise delivery fee list
        /// </summary>
        public IList<VendorWiseDeliveryFee> VendorWiseDeliveryFees { get; set; }

        /// <summary>
        /// Gets or sets Tip fee
        /// </summary>
        public string TipFee { get; set; }

        /// <summary>
        /// Gets or sets vendor wise Tip fee list
        /// </summary>
        public IList<VendorWiseTipFee> VendorWiseTipFees { get; set; }

        /// <summary>
        /// Gets or sets Tip Type Id
        /// </summary>
        public int TipTypeId { get; set; }

        /// <summary>
        /// Gets or sets Custom Tip Amount
        /// </summary>
        public decimal CustomTipAmount { get; set; }

        /// <summary>
        /// Gets or sets Available Tip Types list
        /// </summary>
        public IList<SelectListItem> AvailableTipTypes { get; set; }
    }
}