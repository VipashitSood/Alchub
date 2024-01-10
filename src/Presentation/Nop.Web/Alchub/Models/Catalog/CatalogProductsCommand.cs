using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.UI.Paging;

namespace Nop.Web.Models.Catalog
{
    /// <summary>
	/// Filtering and paging model for catalog
	/// </summary>
	public partial record CatalogProductsCommand : BasePageableModel
    {
        #region Ctor

        /// <summary>
        /// Constructor
        /// </summary>
        public CatalogProductsCommand()
        {
            SpecificationFilter = new CustomSpecificationFilterModel();
            SearchTermFilter = new SearchTermFilterModel();
            AvailableSortOptions = new List<SelectListItem>();
            PageSizeOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Available page size options
        /// </summary>
        public IList<SelectListItem> PageSizeOptions { get; set; }

        /// <summary>
        /// Available sort options
        /// </summary>
        public IList<SelectListItem> AvailableSortOptions { get; set; }

        /// <summary>
        /// A value indicating whether customers are allowed to select page size
        /// </summary>
        public bool AllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// SearchTerm
        /// </summary>
        public SearchTermFilterModel SearchTermFilter { get; set; }

        public CustomSpecificationFilterModel SpecificationFilter { get; set; }

        #endregion

        #region Nested classes

        /// <summary>
        /// Represents a specification filter model
        /// </summary>
        public partial record CustomSpecificationFilterModel : BaseNopModel
        {
            #region Constants
            private const string QUERYSTRINGPARAM = "specs";
            #endregion

            #region Utilities

            /// <summary>
            /// Get IDs of already filtered specification options
            /// </summary>
            /// <param name="webHelper">Web helper</param>
            /// <returns>IDs</returns>
            public virtual List<int> GetAlreadyFilteredSpecOptionIds(IWebHelper webHelper)
            {
                var result = new List<int>();

                var alreadyFilteredSpecsStr = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (string.IsNullOrWhiteSpace(alreadyFilteredSpecsStr))
                    return result;

                foreach (var spec in alreadyFilteredSpecsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int.TryParse(spec.Trim(), out int specId);
                    if (!result.Contains(specId))
                        result.Add(specId);
                }
                return result;
            }

            #endregion
        }

        /// <summary>
        ///  SearchTerm filter item
        /// </summary>
        public partial record SearchTermFilterModel : BaseNopModel
        {
            #region Constraints
            private const string QCATEGORYID = "cid";

            private const string QSUBCATEGORYID = "subCatid";

            private const string QVENDORID = "vendorid";

            private const string QMANUFACTURERID = "mid";

            private const string QPriceRange = "price";

            private const string QSEARCHTERM = "q";
            #endregion

            #region Properties

            public bool Enabled { get; set; }

            public SearchTermFilterItem SearchTermFilterItem { get; set; }

            public CategoryIdsFilterItem CategoryIdsFilterItem { get; set; }

            public SubCategoryIdFilterItem SubCategoryIdFilterItem { get; set; }

            public VendorIdsFilterItem VendorIdsFilterItem { get; set; }

            public ManufacturerIdsFilterItem ManufacturerIdsFilterItem { get; set; }
            #endregion

            #region Methods

            /// <summary>
            /// Get SearchTerm
            /// </summary>
            /// <param name="webHelper">Web helper</param>
            /// <returns>IDs</returns>
            public virtual Task<string> GetSearchTerm(IWebHelper webHelper)
            {
                var result = string.Empty;

                var searchTerm = webHelper.QueryString<string>(QSEARCHTERM);
                if (!string.IsNullOrWhiteSpace(searchTerm))
                    return Task.FromResult(searchTerm);

                return Task.FromResult(result);
            }

            /// <summary>
            /// Get Category Id
            /// </summary>
            /// <param name="webHelper">Web helper</param>
            /// <returns>IDs</returns>
            public virtual Task<string> GetCategoryIds(IWebHelper webHelper)
            {
                var result = string.Empty;

                var catid = webHelper.QueryString<string>(QCATEGORYID);
                if (!string.IsNullOrWhiteSpace(catid))
                    return Task.FromResult(catid);

                return Task.FromResult(result);
            }

            /// <summary>
            /// Get Sub Category Id
            /// </summary>
            /// <param name="webHelper">Web helper</param>
            /// <returns>IDs</returns>
            public virtual Task<string> GetSubCategoryId(IWebHelper webHelper)
            {
                var result = string.Empty;

                var subCategoryId = webHelper.QueryString<string>(QSUBCATEGORYID);
                if (!string.IsNullOrWhiteSpace(subCategoryId))
                    return Task.FromResult(subCategoryId);

                return Task.FromResult(result);
            }

            /// <summary>
            /// Get Vendor Id
            /// </summary>
            /// <param name="webHelper">Web helper</param>
            /// <returns>IDs</returns>
            public virtual Task<string> GetVendorIds(IWebHelper webHelper)
            {
                var result = string.Empty;

                var vendorId = webHelper.QueryString<string>(QVENDORID);
                if (!string.IsNullOrWhiteSpace(vendorId))
                    return Task.FromResult(vendorId);

                return Task.FromResult(result);
            }

            /// <summary>
            /// Get Vendor Id
            /// </summary>
            /// <param name="webHelper">Web helper</param>
            /// <returns>IDs</returns>
            public virtual Task<string> GetManufacturerIds(IWebHelper webHelper)
            {
                var result = string.Empty;

                var vendorId = webHelper.QueryString<string>(QMANUFACTURERID);
                if (!string.IsNullOrWhiteSpace(vendorId))
                    return Task.FromResult(vendorId);

                return Task.FromResult(result);
            }

            public virtual Task<string> GetPriceRange(IWebHelper webHelper)
            {
                var result = string.Empty;

                var price = webHelper.QueryString<string>(QPriceRange);
                if (!string.IsNullOrWhiteSpace(price))
                    return Task.FromResult(price);

                return Task.FromResult(result);
            }

            public virtual Task PrepareSearchTerm(string searchTerm)
            {
                Enabled = !string.IsNullOrEmpty(searchTerm);
                SearchTermFilterItem = new SearchTermFilterItem() { SearchTerm = searchTerm };
                return Task.CompletedTask;
            }

            public virtual Task PrepareCategoryIdsSearch(string categoryIds)
            {
                CategoryIdsFilterItem = new CategoryIdsFilterItem() { CategoryIdsSearch = categoryIds };
                return Task.CompletedTask;
            }

            public virtual Task PrepareSubCategoryIdsSearch(string subCategoryIdsSearch)
            {
                SubCategoryIdFilterItem = new SubCategoryIdFilterItem() { SubCategoryIdSearch = subCategoryIdsSearch };
                return Task.CompletedTask;
            }

            public virtual Task PrepareVendorIdsSearch(string vendorIds)
            {
                VendorIdsFilterItem = new VendorIdsFilterItem() { VendorIdSearch = vendorIds };
                return Task.CompletedTask;
            }

            public virtual Task PrepareManufacturerIdsSearch(string manufacturerIds)
            {
                ManufacturerIdsFilterItem = new ManufacturerIdsFilterItem() { ManufacturerIdSearch = manufacturerIds };
                return Task.CompletedTask;
            }

            #endregion
        }

        public partial record SearchTermFilterItem : BaseNopModel
        {
            public string SearchTerm { get; set; }
        }

        /// <summary>
        /// CategoryId filter item
        /// </summary>
        public partial record CategoryIdsFilterItem : BaseNopModel
        {
            /// <summary>
            /// Category Ids Search
            /// </summary>
            public string CategoryIdsSearch { get; set; }
        }

        /// <summary>
        /// Sub Category Id filter item
        /// </summary>
        public partial record SubCategoryIdFilterItem : BaseNopModel
        {
            /// <summary>
            /// Sub Category Id Search
            /// </summary>
            public string SubCategoryIdSearch { get; set; }
        }

        /// <summary>
        /// Vendor id filter item
        /// </summary>
        public partial record VendorIdsFilterItem : BaseNopModel
        {
            /// <summary>
            /// Vendor Ids Search
            /// </summary>
            public string VendorIdSearch { get; set; }
        }

        /// <summary>
        /// Manufacturer id filter item
        /// </summary>
        public partial record ManufacturerIdsFilterItem : BaseNopModel
        {
            /// <summary>
            /// Manufacturer Ids Search
            /// </summary>
            public string ManufacturerIdSearch { get; set; }
        }

        #endregion
    }
}
