using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Order
{
    public partial record ShipmentDetailsModel : BaseNopEntityModel
    {
		#region Nested Classes

        public partial record ShipmentItemModel : BaseNopEntityModel
        {
            /// <summary>
            /// Gets or sets the custom attributes xml (for grouped product variant)
            /// </summary>
            public string CustomAttributeInfo { get; set; }
        }

		#endregion
    }
}