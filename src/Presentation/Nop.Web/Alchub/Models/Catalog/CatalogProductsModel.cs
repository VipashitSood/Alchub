using System.Collections.Generic;
using Nop.Web.Framework.UI.Paging;

namespace Nop.Web.Models.Catalog
{
    /// <summary>
    /// Represents a catalog products model
    /// </summary>
    public partial record CatalogProductsModel : BasePageableModel
    {
        #region Ctor

        protected override void PostInitialize()
        {
            VendorsProductCounts = new List<ProductCountCommonModel>();
            ManufacturersProductCounts = new List<ProductCountCommonModel>();
            SpecificationOptionsProductCounts = new List<ProductCountCommonModel>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the vendor product count
        /// Note: We have SelectedListItem in default vendors model, so preparing product count sapertely
        /// </summary>
        public IList<ProductCountCommonModel> VendorsProductCounts { get; set; }

        /// <summary>
        /// Gets or sets the vendor product count
        /// Note: We have SelectedListItem in default vendors model, so preparing product count sapertely
        /// </summary>
        public IList<ProductCountCommonModel> ManufacturersProductCounts { get; set; }

        /// <summary>
        /// Gets or sets the vendor product count
        /// Note: We have SelectedListItem in default vendors model, so preparing product count sapertely
        /// </summary>
        public IList<ProductCountCommonModel> SpecificationOptionsProductCounts { get; set; }

        #endregion

        #region Nested class

        public partial class ProductCountCommonModel
        {
            public int Id { get; set; }
            public int? NumberOfProducts { get; set; }
        }

        #endregion
    }
}