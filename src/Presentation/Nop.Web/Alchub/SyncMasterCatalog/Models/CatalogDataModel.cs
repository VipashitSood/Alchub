using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;

namespace Nop.Web.Alchub.SyncMasterCatalog.Models
{
    public partial class CatalogDataModel
    {
        public CatalogDataModel()
        {
            Categories = new List<SyncCatalogCategoryModel>();
            Manufacturers = new List<SyncCatalogManufacturerModel>();
            SpecificationAttributes = new List<SpecificationAttribute>();
            MasterProducts = new List<SyncCatalogProductModel>();
        }

        [JsonProperty("categories")]
        public IList<SyncCatalogCategoryModel> Categories { get; set; }

        [JsonProperty("manufacturers")]
        public IList<SyncCatalogManufacturerModel> Manufacturers { get; set; }

        [JsonProperty("specification_attributes")]
        public IList<SpecificationAttribute> SpecificationAttributes { get; set; }

        [JsonProperty("master_products")]
        public IList<SyncCatalogProductModel> MasterProducts { get; set; }
    }
}
