using System;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    /// <summary>
    /// Represents a return request model
    /// </summary>
    public partial record ReturnRequestModel : BaseNopEntityModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the custom attributes xml (for grouped product variant)
        /// </summary>
        public string CustomAttributeInfo { get; set; }

        #endregion
    }
}