using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Models.V1.Catalog.Filter;

namespace Nop.Plugin.Api.Factories.V1
{
    /// <summary>
    /// Represents catalog model factory interface v1
    /// </summary>
    public interface ICatalogModelFactory
    {
        /// <summary>
        /// Prepare catalog filter response model
        /// </summary>
        /// <param name="catalogFilterRequest"></param>
        /// <param name="categoryIds"></param>
        /// <returns></returns>
        Task<CatalogFilterResponseModel> PreprareCatalogFilterModel(CatalogFilterRequestModel catalogFilterRequest, Customer customer);

        /// <summary>
        /// Prepare base filter response model
        /// </summary>
        /// <param name="baseFilterRequestModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<BaseFilterResponseModel> PreprareBaseFilterResponseModel(BaseFilterRequestModel baseFilterRequestModel, Customer customer);

        /// <summary>
        /// Prepare manufacturer filter response model
        /// </summary>
        /// <param name="manufacturerFilterRequestModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<ManufacturerFilterResponseModel> PreprareManufacturerFilterResponseModel(ManufacturerFilterRequestModel manufacturerFilterRequestModel, Customer customer);

        /// <summary>
        /// Prepare vendor filter response model
        /// </summary>
        /// <param name="vendorFilterRequestModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<VendorFilterResponseModel> PreprareVendorFilterResponseModel(VendorFilterRequestModel vendorFilterRequestModel, Customer customer);

        /// <summary>
        /// Prepare specification attribute options filter response model
        /// </summary>
        /// <param name="specOptionsFilterRequestModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<SpecificationAttributeOptionFilterResponseModel> PreprareSpecificationAttributeOptionFilterResponseModel(SpecificationAttributeOptionFilterRequestModel specOptionsFilterRequestModel, Customer customer);

        /// <summary>
        /// Prepare filter root categories response model
        /// </summary>
        /// <returns></returns>
        Task<RootCategoriesResponseModel> PreprareRootCategoriesResponseModel();
    }
}
