using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    /// <summary>
    /// Represents a shipment item model
    /// </summary>
    public partial record ShipmentItemModel : BaseNopEntityModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the custom attributes xml (for grouped product variant)
        /// </summary>
        public string CustomAttributeInfo { get; set; }

        #endregion
    }
}