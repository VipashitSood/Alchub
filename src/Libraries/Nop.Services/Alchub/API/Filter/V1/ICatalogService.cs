using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Alchub.API.Filter.V1
{
    /// <summary>
    /// Represents api catalog service interface 
    /// </summary>
    public interface ICatalogService
    {
        /// <summary>
        /// Get all filter result
        /// </summary>
        /// <param name="categoryIds"></param>
        /// <param name="vendorIds"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name="specificationAttributeOptionIds"></param>
        /// <param name=""></param>
        /// <returns></returns>
        Task<IList<AllFilterResult>> GetAllFilterResultAsync(
            List<int> categoryIds,
            List<int> vendorIds,
            List<int> manufacturerIds = null,
            List<int> specificationAttributeOptionIds = null);

        /// <summary>
        /// Get base filter result
        /// </summary>
        /// <param name="rootCategoryId"></param>
        /// <param name="vendorIds"></param>
        /// <param name="subCategoryIds"></param>
        /// <param name=""></param>
        /// <returns></returns>
        Task<IList<BaseFilterResult>> GetBaseFilterResultAsync(
            int rootCategoryId,
            List<int> vendorIds,
            List<int> subCategoryIds = null);

        /// <summary>
        /// Get manufacturers filter result
        /// </summary>
        /// <param name="rootCategoryId"></param>
        /// <param name="vendorIds"></param>
        /// <param name="subCategoryIds"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name=""></param>
        /// <returns></returns>
        Task<IList<ManufacturerFilterResult>> GetManufacturerFilterResultAsync(
            int rootCategoryId,
            List<int> vendorIds,
            List<int> subCategoryIds = null,
            List<int> manufacturerIds = null);

        /// <summary>
        /// Get vendors filter result
        /// </summary>
        /// <param name="rootCategoryId"></param>
        /// <param name="vendorIds"></param>
        /// <param name="subCategoryIds"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name="availableVendorIds"></param>
        /// <param name=""></param>
        /// <returns></returns>
        Task<IList<VendorFilterResult>> GetVendorFilterResultAsync(
            int rootCategoryId,
            List<int> vendorIds,
            List<int> subCategoryIds = null,
            List<int> manufacturerIds = null,
            List<int> availableVendorIds = null);

        /// <summary>
        /// Get specification attribute options filter result
        /// </summary>
        /// <param name="rootCategoryId"></param>
        /// <param name="specificationAttributeId"></param>
        /// <param name="vendorIds"></param>
        /// <param name="subCategoryIds"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name="specificationAttOptionIds"></param>
        /// <param name=""></param>
        /// <returns></returns>
        Task<IList<SpecificationAttributeOptionFilterResult>> GetSpecificationAttributeOptionFilterResultAsync(
            int rootCategoryId,
            int specificationAttributeId,
            List<int> vendorIds,
            List<int> subCategoryIds = null,
            List<int> manufacturerIds = null,
            List<int> specificationAttOptionIds = null);

    }
}