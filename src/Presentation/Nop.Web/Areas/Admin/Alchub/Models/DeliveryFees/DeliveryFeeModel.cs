using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.DeliveryFees
{
    public partial record DeliveryFeeModel : BaseNopEntityModel
    {
        public DeliveryFeeModel()
        {
            AvailableVendors = new List<SelectListItem>();
            AvailableDeliveryFeeTypes = new List<SelectListItem>();
        }

        /// <summary>
        /// Gets or sets Is Admin
        /// </summary>
        [NopResourceDisplayName("Alchub.Admin.DeliveryFee.Fields.IsAdmin")]
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Gets or sets Vendor Id
        /// </summary>
        [NopResourceDisplayName("Alchub.Admin.DeliveryFee.Fields.VendorId")] 
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets Delivery Fee Type Id
        /// </summary>
        [NopResourceDisplayName("Alchub.Admin.DeliveryFee.Fields.DeliveryFeeTypeId")] 
        public int DeliveryFeeTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Fixed Fee
        /// </summary>
        [NopResourceDisplayName("Alchub.Admin.DeliveryFee.Fields.FixedFee")] 
        public decimal FixedFee { get; set; }

        /// <summary>
        /// Gets or sets the Dynamic Base Fee
        /// </summary>
        [NopResourceDisplayName("Alchub.Admin.DeliveryFee.Fields.DynamicBaseFee")] 
        public decimal DynamicBaseFee { get; set; }

        /// <summary>
        /// Gets or sets the Dynamic Base Distance
        /// </summary>
        [NopResourceDisplayName("Alchub.Admin.DeliveryFee.Fields.DynamicBaseDistance")]
        public decimal DynamicBaseDistance { get; set; }

        /// <summary>
        /// Gets or sets the Dynamic Extra Fee
        /// </summary>
        [NopResourceDisplayName("Alchub.Admin.DeliveryFee.Fields.DynamicExtraFee")]
        public decimal DynamicExtraFee { get; set; }

        /// <summary>
        /// Gets or sets the Dynamic Maximum Fee
        /// </summary>
        [NopResourceDisplayName("Alchub.Admin.DeliveryFee.Fields.DynamicMaximumFee")]
        public decimal DynamicMaximumFee { get; set; }

        /// <summary>
        /// Gets or sets the Available Vendors
        /// </summary>
        public IList<SelectListItem> AvailableVendors { get; set; }

        /// <summary>
        /// Gets or sets the Available Delivery Fee Types
        /// </summary>
        public IList<SelectListItem> AvailableDeliveryFeeTypes { get; set; }
    }
}