using Newtonsoft.Json;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTO.Categories;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTOs.Categories
{
    public class CategoriesHierarchybject : ISerializableObject
    {
        [JsonProperty("categories")]
        public IList<CategoryHierarchyModel> Categories { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "categories";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(CategoriesHierarchybject);
        }
    }

    public class CategoryHierarchyModel : CategoryDto
    {
        public CategoryHierarchyModel()
        {
            SubCategories = new List<CategoryHierarchyModel>();
        }

        [JsonProperty("sub_categories")]
        public List<CategoryHierarchyModel> SubCategories { get; set; }

        #region Filter properties

        //Note: This is for filter purpose, These values were getting assigned only if required params are available

        /// <summary>
        /// Selected: This is helpfull to set if category is selected in filter 
        /// </summary>
        [JsonProperty("is_selected")]
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets number of products in category
        /// </summary>
        [JsonProperty("number_of_products")]
        public int? NumberOfProducts { get; set; }

        #endregion
    }

    public class FilterCategoryHierarchyModel : BaseDto
    {
        public FilterCategoryHierarchyModel()
        {
            SubCategories = new List<FilterCategoryHierarchyModel>();
        }

        [JsonProperty("sub_categories")]
        public List<FilterCategoryHierarchyModel> SubCategories { get; set; }

        #region Filter properties

        //Note: This is for filter purpose, These values were getting assigned only if required params are available
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Selected: This is helpfull to set if category is selected in filter 
        /// </summary>
        [JsonProperty("is_selected")]
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets number of products in category
        /// </summary>
        [JsonProperty("number_of_products")]
        public int? NumberOfProducts { get; set; }

        #endregion
    }
}
