using System.Collections.Generic;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    /// <summary>
    /// Represents a vendor extended model
    /// </summary>
    public partial record VendorModel
    {
        #region Ctor
        public VendorModel()
        {
            if (PageSize < 1)
                PageSize = 5;

            Address = new AddressModel();
            VendorAttributes = new List<VendorAttributeModel>();
            Locales = new List<VendorLocalizedModel>();
            AssociatedCustomers = new List<VendorAssociatedCustomerModel>();
            VendorNoteSearchModel = new VendorNoteSearchModel();
            VendorTimingSearchModel = new VendorTimingSearchModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.ManageDelivery")]
        public bool ManageDelivery { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.PickAvailable")]
        public bool PickAvailable { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.MinimumOrderAmount")]
        public decimal MinimumOrderAmount { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.GeoLocationCoordinates")]
        public string GeoLocationCoordinates { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.GeoFencingCoordinates")]
        public string GeoFencingCoordinates { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.OrderTax")]
        public decimal OrderTax { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.DeliveryAvailable")]
        public bool DeliveryAvailable { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.PickupAddress")]
        public string PickupAddress { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.FtpFilePath")]
        public string FtpFilePath { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.GeoFenceShapeTypeId")]
        public int GeoFenceShapeTypeId { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.Fields.RadiusDistance")]
        public decimal RadiusDistance { get; set; }

        public VendorTimingSearchModel VendorTimingSearchModel { get; set; }
        #endregion
    }
}