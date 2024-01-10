using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    /// <summary>
    /// Represents an order item model
    /// </summary>
    public partial record OrderItemModel : BaseNopEntityModel
    {

        #region Properties

        /// <summary>
        /// Gets or sets the slot identifiers
        /// </summary>
        public int SlotId { get; set; }

        /// <summary>
        /// Gets or sets the SlotPrice
        /// </summary>
        [NopResourceDisplayName("Admin.Orders.Fields.SlotPrice")]
        public decimal SlotPrice { get; set; }

        /// <summary> 
        /// Gets or sets the StartTime
        /// </summary>
        [NopResourceDisplayName("Admin.Orders.Fields.SlotStartTime")]
        public string SlotStartTime { get; set; }

        /// <summary>
        /// Gets or sets the EndTime
        /// </summary>
        [NopResourceDisplayName("Admin.Orders.Fields.SlotTime")]
        public string SlotTime { get; set; }


        public bool InPickup { get; set; }

        /// <summary>
        /// Gets or sets the custom attributes xml (for grouped product variant)
        /// </summary>
        public string CustomAttributeInfo { get; set; }

        /// <summary>
        /// Gets or sets the custom attributes xml (for grouped product variant)
        /// </summary>
        public string OrderItemStatus { get; set; }
        public int OrderItemStatusId { get; set; }

        /// <summary>
        /// specify if this products venodr manage delivery or not 
        /// </summary>
        public bool IsVendorManageDelivery { get; set; }

        #endregion
    }
}