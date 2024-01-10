using System.Collections.Generic;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.TipFees;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    /// <summary>
    /// Represents a order model
    /// </summary>
    public partial record OrderModel : BaseNopEntityModel
    {
        #region Ctor

        public OrderModel()
        {
            CustomValues = new Dictionary<string, object>();
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            Items = new List<OrderItemModel>();
            UsedDiscounts = new List<UsedDiscountModel>();
            OrderShipmentSearchModel = new OrderShipmentSearchModel();
            OrderNoteSearchModel = new OrderNoteSearchModel();
            BillingAddress = new AddressModel();
            ShippingAddress = new AddressModel();
            PickupAddress = new AddressModel();
            VendorWiseDeliveryFees = new List<VendorWiseDeliveryFee>();
            VendorWiseTipFees = new List<VendorWiseTipFee>();
        }

        #endregion

        [NopResourceDisplayName("Admin.Orders.Fields.ServiceFee")]
        public string ServiceFee { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.SlotFee")]
        public string SlotFee { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Orders.Fields.DeliveryFee")]
        public string DeliveryFee { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Orders.Fields.TipFee")]
        public string TipFee { get; set; }

        /// <summary>
        /// Gets or sets vendor wise delivery fee list
        /// </summary>
        public IList<VendorWiseDeliveryFee> VendorWiseDeliveryFees { get; set; }

        /// <summary>
        /// Gets or sets vendor wise Tip fee list
        /// </summary>
        public IList<VendorWiseTipFee> VendorWiseTipFees { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.TotalRefundAmount")]
        public string TotalRefundAmount { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Orders.Fields.TotalCreditAmount")]
        public string TotalCreditAmount { get; set; }
    }
}