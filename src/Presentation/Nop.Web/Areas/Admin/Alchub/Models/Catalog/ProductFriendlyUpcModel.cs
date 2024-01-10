using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Alchub.Models.Catalog
{
    /// <summary>
    /// Represents a product friendly upc model
    /// </summary>
    public partial record ProductFriendlyUpcModel : BaseNopEntityModel
    {
        public int MasterProductId { get; set; }

        public int VendorId { get; set; }

        //picture thumbnail
        [NopResourceDisplayName("Alchub.Admin.Catalog.ProductFriendlyUpc.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Catalog.ProductFriendlyUpc.Fields.ProductName")]
        public string ProductName { get; set; }
        
        [NopResourceDisplayName("Alchub.Admin.Catalog.ProductFriendlyUpc.Fields.OriginalUPCCode")]
        public string OriginalUPCCode { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Catalog.ProductFriendlyUpc.Fields.Sku")]
        public string Sku { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Catalog.ProductFriendlyUpc.Fields.Szie")]
        public string Size { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Catalog.ProductFriendlyUpc.Fields.Category")]
        public string Category { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Catalog.ProductFriendlyUpc.Fields.FriendlyUPCCode")]
        public string FriendlyUPCCode { get; set; }
    }
}
