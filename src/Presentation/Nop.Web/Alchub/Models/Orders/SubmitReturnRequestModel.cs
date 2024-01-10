using System;
using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Order
{
    public partial record SubmitReturnRequestModel : BaseNopModel
    {
        #region Nested classes

        public partial record OrderItemModel : BaseNopEntityModel
        {
            /// <summary>
            /// Gets or sets the custom attributes xml (for grouped product variant)
            /// </summary>
            public string CustomAttributeInfo { get; set; }
        }

        #endregion
    }

}