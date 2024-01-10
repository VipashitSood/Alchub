using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial record ProductDetailsModel
    {
        public ProductDetailsModel()
        {
            DefaultPictureModel = new PictureModel();
            PictureModels = new List<PictureModel>();
            GiftCard = new GiftCardModel();
            ProductPrice = new ProductPriceModel();
            AddToCart = new AddToCartModel();
            ProductAttributes = new List<ProductAttributeModel>();
            AssociatedProducts = new List<ProductDetailsModel>();
            VendorModel = new VendorBriefInfoModel();
            Breadcrumb = new ProductBreadcrumbModel();
            ProductTags = new List<ProductTagModel>();
            ProductSpecificationModel = new ProductSpecificationModel();
            ProductManufacturers = new List<ManufacturerBriefInfoModel>();
            ProductReviewOverview = new ProductReviewOverviewModel();
            TierPrices = new List<TierPriceModel>();
            ProductEstimateShipping = new ProductEstimateShippingModel();

            //vendors
            AvailableVendors = new List<SelectListItem>();
            SlotsManagement = new List<dynamic>();
            ProductVendors = new List<ProductVendorModel>();
            VendorSortingOptions = new List<SelectListItem>();
            ProductReviews = new ProductReviewsModel();

            GroupedProductVariants = new List<GroupedProductVariantBrifInfoModel>();
        }

        //vendor(s)
        public int VendorId { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        public int VendorSort { get; set; }
        public IEnumerable<dynamic> SlotsManagement { get; set; }

        //sub products 
        public IList<ProductVendorModel> ProductVendors { get; set; }

        public IList<SelectListItem> VendorSortingOptions { get; set; }

        /// <summary>
        /// Product size (It will be used for grouped product variant)
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Product container (It will be used for grouped product variant)
        /// </summary>
        public string Container { get; set; }

        //Product reviews
        public ProductReviewsModel ProductReviews { get; set; }

        //Published
        public bool Published { get; set; }

        //grouped product variants
        public IList<GroupedProductVariantBrifInfoModel> GroupedProductVariants { get; set; }

        #region Nested Classes

        public partial record ProductVendorModel
        {
            public ProductVendorModel()
            {
                VendorProductPrice = new ProductPriceModel();
            }
            public int ProductId { get; set; }
            public int VendorId { get; set; }
            public string VendorName { get; set; }
            public bool ManageDelivery { get; set; }
            public bool DeliveryAvailable { get; set; }
            public bool PickAvailable { get; set; }
            public string OrderAmount { get; set; }
            public string DeliveryFee { get; set; }
            public string Distance { get; set; }
            public decimal DistanceValue { get; set; }
            //price 
            public ProductPriceModel VendorProductPrice { get; set; }
            public DateTime? StartTime { get; set; }
        }

        public partial record ProductPriceModel
        {
            public decimal PriceWithoutDiscount { get; set; }
        }

        public partial record AddToCartModel
        {
            public bool IsWishlist { get; set; }
        }

        public partial record GroupedProductVariantBrifInfoModel
        {
            /// <summary>
            /// Grouped product identifiers
            /// </summary>
            public int GroupedProductId { get; set; }

            /// <summary>
            /// Variant name. (Specification attribute option name.)
            /// </summary>
            public string VariantName { get; set; }

            /// <summary>
            /// Grouped product Sename
            /// </summary>
            public string SeName { get; set; }

            /// <summary>
            /// Display order
            /// </summary>
            public int DisplayOrder { get; set; }

            /// <summary>
            /// Is active (current product details page grouped product ?)
            /// </summary>
            public bool IsActive { get; set; }
        }

        #endregion
    }
}