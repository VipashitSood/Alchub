using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Services.ExportImport;

namespace Nop.Web.Alchub.SyncMasterCatalog.Models
{
    public class SyncCatalogProductModel
    {
        public SyncCatalogProductModel()
        {
            ProductCategories = new List<CatalogProductCategoryModel>();
            ProductManufacturers = new List<CatalogProductManufacturesModel>();
            ProductSpecificationAttributes = new CatalogProductSpecificationAttributeModel();
            AssociatedProducts = new CatalogAssociatedProductModel();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string SKU { get; set; }

        public string UPCCode { get; set; }

        public string ShortDescription { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public string SeName { get; set; }

        public bool Published { get; set; }

        public string Size { get; set; }

        public string Container { get; set; }

        public string FullDescription { get; set; }

        public int DisplayOrder { get; set; }

        public bool VisibleIndividually { get; set; }

        public bool Deleted { get; set; }

        public int ProductTypeId { get; set; }

        public IList<CatalogProductCategoryModel> ProductCategories { get; set; }

        public IList<CatalogProductManufacturesModel> ProductManufacturers { get; set; }

        public CatalogProductSpecificationAttributeModel ProductSpecificationAttributes { get; set; }

        public CatalogAssociatedProductModel AssociatedProducts { get; set; }

        #region Nested classes

        public class CatalogProductCategoryModel : ProductCategory
        {
            public string CategoryName { get; set; }
        }

        public class CatalogProductManufacturesModel : ProductManufacturer
        {
            public string ManufactureName { get; set; }
        }

        public class CatalogProductSpecificationAttributeModel : ImportSpecificationAttribute
        {
            //keep same properties as in import manager method. 
        }

        public class CatalogAssociatedProductModel
        {
            public CatalogAssociatedProductModel()
            {
                AssociatedProducts = new List<AssociatedProductModel>();
            }
            public IList<AssociatedProductModel> AssociatedProducts { get; set; }

            #region Nested

            public class AssociatedProductModel
            {
                public int ProductId { get; set; }
                public string SKU { get; set; }
                public string UPCCode { get; set; }
                public int DisplayOrder { get; set; }
            }

            #endregion
        }

        #endregion
    }
}
