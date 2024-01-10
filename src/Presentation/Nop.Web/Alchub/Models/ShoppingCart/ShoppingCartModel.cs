using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.ShoppingCart
{
    public partial record ShoppingCartModel
    {
        public ShoppingCartModel()
        {
            Items = new List<ShoppingCartItemModel>();
            Warnings = new List<string>();
            DiscountBox = new DiscountBoxModel();
            GiftCardBox = new GiftCardBoxModel();
            CheckoutAttributes = new List<CheckoutAttributeModel>();
            OrderReviewData = new OrderReviewDataModel();
            ButtonPaymentMethodViewComponentNames = new List<string>();
            ShoppingCartVendors = new List<ShoppingCartVendorModel>();
        }

        /// <summary>
        /// Get or set the list of Vendors
        /// </summary>
        public IList<ShoppingCartVendorModel> ShoppingCartVendors { get; set; }

        #region Nested Classes

        public partial record ShoppingCartItemModel
        {
            public string SlotPrice { get; set; }

            public string SlotStartDate { get; set; }

            public string SlotTime { get; set; }

            public bool IsPickup { get; set; }

            /// <summary>
            /// Get or set the Vendor Id
            /// </summary>
            public int VendorId { get; set; }

            public string CustomAttributeInfo { get; set; }
        }

        public partial record ShoppingCartVendorModel
        {
            public ShoppingCartVendorModel()
            {
                Warnings = new List<string>();
            }

            /// <summary>
            /// Get or set the Vendor Id
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Get or set the Vendor Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Get or set the Warnings list
            /// </summary>
            public IList<string> Warnings { get; set; }
        }

        #endregion
    }
}