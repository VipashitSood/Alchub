using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Represents default values related to catalog services
    /// </summary>
    public static partial class NopCatalogDefaults
    {
        #region Caching defaults

        #region Products

        /// <summary>
        /// Key for "same upc code" product.
        /// </summary>
        /// <remarks>
        /// {0} : upc code
        /// {1} : Exclude master product
        /// </remarks>
        public static CacheKey SameUpcProductsCacheKey => new("Alchub.Nop.same.upc.product.byUpc.{0}-{1}", SameUpcProductsPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : upc code
        /// </remarks>
        public static string SameUpcProductsPrefix => "Alchub.Nop.same.upc.product.byUpc.{0}";

        /// <summary>
        /// Key for "related" product displayed on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : current product id
        /// {1} : show hidden records?
        /// {2} : show master products?
        /// </remarks>
        public static CacheKey RelatedProductsCacheKey => new("Nop.relatedproduct.byproduct.{0}-{1}-{2}", RelatedProductsPrefix);

        /// <summary>
        /// Key for "upccode product picture" .
        /// </summary>
        /// <remarks>
        /// {0} : picture ID
        /// </remarks>
        public static CacheKey UpccodeProductPictureCacheKey => new("Alchub.Nop.product.UpcCode.ByProductPicture.{0}", UpccodeProductPicturePrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : picture ID
        /// </remarks>
        public static string UpccodeProductPicturePrefix => "Alchub.Nop.product.UpcCode.ByProductPicture.{0}";

        #endregion

        #region API

        /// <summary>
        /// Key for caching api all filter data.
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : available vendor Ids
        /// </remarks>
        public static CacheKey ApiAllFilterDataKey => new("Alchub.Nop.api.allfilter.bycategory.{0}-{1}", ApiAllFilterDataPrefix);
        public static string ApiAllFilterDataPrefix => "Alchub.Nop.api.allfilter.bycategory.{0}";

        #endregion

        #region Categories

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer roles ID hash
        /// {1} : current store ID
        /// {2} : categories ID hash
        /// {3} : vendors ID hash
        /// </remarks>
        public static CacheKey CategoryProductsNumberByVendorCacheKey => new("Nop.productcategory.products.by.vendors.number.{0}-{1}-{2}-{3}", CategoryProductsNumberByVendorPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CategoryProductsNumberByVendorPrefix => "Nop.productcategory.products.by.vendors.number.";

        #endregion

        #region Vendors

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// {1} : show hidden records?
        /// {2} : check slot created?
        /// </remarks>
        public static CacheKey VendorsAllCacheKey => new("Nop.vendor.all.{0}-{1}-{2}", NopEntityCacheDefaults<Vendor>.AllPrefix);

        #endregion

        #endregion
    }
}