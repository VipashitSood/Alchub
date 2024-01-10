using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    /// <summary>
    /// Represent filter root category response model
    /// </summary>
    public partial class RootCategoriesResponseModel
    {
        #region Ctor

        public RootCategoriesResponseModel()
        {
            RootCategories = new List<FilterRootCategoryModel>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the categories
        /// </summary>
        [JsonProperty("root_categories")]
        public IList<FilterRootCategoryModel> RootCategories { get; set; }

        #endregion

        #region Nested classes

        #region Manufacture

        public record FilterRootCategoryModel : BaseFilterEntityModel
        {
            /// <summary>
            /// Gets or sets the category name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the display order
            /// </summary>
            [JsonProperty("display_order")]
            public int DsiplayOrder { get; set; }
        }

        #endregion

        #endregion
    }
}
