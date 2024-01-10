using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial record CategoryModel : BaseNopEntityModel
    {
        public CategoryModel()
        {
            PictureModel = new PictureModel();
            FeaturedProducts = new List<ProductOverviewModel>();
            SubCategories = new List<SubCategoryModel>();
            Vendors = new List<VendorModel>();
            Products = new List<Product>();
            CategoryBreadcrumb = new List<CategoryModel>();
            CatalogProductsModel = new CatalogProductsModel();
            CatalogProductsCommand = new CatalogProductsCommand();
        }


        /// <summary>
        /// Gets or sets number of products in category
        /// </summary>
        public int? NumberOfProducts { get; set; }

        /// <summary>
        /// Gets or sets parent category identifier 
        /// </summary>
        public int? ParentCategryId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether category is selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets display order
        /// </summary>
        public int DisplayOrder { get; set; }

        public CatalogProductsModel CatalogProductsModel { get; set; }

        public IList<SubCategoryModel> SubCategories { get; set; }

        public IList<VendorModel> Vendors { get; set; }

        public IList<Product> Products { get; set; }

        public CatalogProductsCommand CatalogProductsCommand { get; set; }

        #region Nested Classes
        public partial record SubCategoryModel : BaseNopEntityModel
        {
            public bool DisplayCategoryFilter { get; set; }

            public int ParentCategoryId { get; set; }

            public int NumberOfProducts { get; set; }

            public string MinProductPrice { get; set; }
        }

        #endregion
    }
}