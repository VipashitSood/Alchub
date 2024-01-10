using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Data;

namespace Nop.Services.Alchub.API.Filter.V1
{
    public class CatalogService : ICatalogService
    {
        #region Fields

        private readonly INopDataProvider _dataProvider;

        #endregion

        #region Ctor

        public CatalogService(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion

        #region Utilities

        private string PrepareCommaSeparatedIds(List<int> ids = null)
        {
            //validate "ids" parameter
            if (ids != null && ids.Contains(0))
                ids.Remove(0);

            //pass identifiers as comma-delimited string
            string commaSeparatedIds = ids != null && ids.Any() ? string.Join(",", ids) : "";
            return commaSeparatedIds;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all filter result
        /// </summary>
        /// <param name="categoryIds"></param>
        /// <param name="vendorIds"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name="specificationAttributeOptionIds"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<IList<AllFilterResult>> GetAllFilterResultAsync(
        List<int> categoryIds,
        List<int> vendorIds,
        List<int> manufacturerIds = null,
        List<int> specificationAttributeOptionIds = null)
        {
            //validate "categoryIds" parameter
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);
            //pass category identifiers as comma-delimited string
            string commaSeparatedCategoryIds = categoryIds == null ? "" : string.Join(",", categoryIds);

            //validate "vendorIds" parameter
            if (vendorIds != null && vendorIds.Contains(0))
                vendorIds.Remove(0);
            //pass vendor identifiers as comma-delimited string
            string commaSeparatedVendorIds = vendorIds == null ? "" : string.Join(",", vendorIds);

            //validate "manufacturerIds" parameter
            if (manufacturerIds != null && manufacturerIds.Contains(0))
                manufacturerIds.Remove(0);
            //pass vendor identifiers as comma-delimited string
            string commaSeparatedManufacturerIds = manufacturerIds == null ? "" : string.Join(",", manufacturerIds);

            //validate "specificationAttributeOptionIds" parameter
            if (specificationAttributeOptionIds != null && specificationAttributeOptionIds.Contains(0))
                specificationAttributeOptionIds.Remove(0);
            //pass specification identifiers as comma-delimited string
            string commaSeparatedSpecIds = specificationAttributeOptionIds == null ? "" : string.Join(",", specificationAttributeOptionIds);

            //prepare sp parametors
            var pCategoryIds = new DataParameter()
            {
                Name = "CategoryIds",
                Value = commaSeparatedCategoryIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pVendorIds = new DataParameter()
            {
                Name = "VendorIds",
                Value = commaSeparatedVendorIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pManufactureIds = new DataParameter()
            {
                Name = "ManufactureIds",
                Value = commaSeparatedManufacturerIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pSpecificationAttributeOptionIds = new DataParameter()
            {
                Name = "SpecificationAttibuteOptionIds",
                Value = commaSeparatedSpecIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            try
            {
                var result = await _dataProvider.QueryProcAsync<AllFilterResult>(
                "ApiFilterLoadAll",
                pCategoryIds,
                pVendorIds,
                pManufactureIds,
                pSpecificationAttributeOptionIds);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Get base filter result
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="vendorIds"></param>
        /// <param name="subCategoryIds"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<IList<BaseFilterResult>> GetBaseFilterResultAsync(
            int rootCategoryId,
            List<int> vendorIds,
            List<int> subCategoryIds = null)
        {
            //validate "vendorIds" parameter
            if (subCategoryIds != null && subCategoryIds.Contains(0))
                subCategoryIds.Remove(0);
            //pass sub categories identifiers as comma-delimited string
            string commaSeparatedSubCategoryIds1 = subCategoryIds == null && subCategoryIds.Any() ? "" : string.Join(",", subCategoryIds);

            //pass sub categories identifiers as comma-delimited string
            string commaSeparatedSubCategoryIds = PrepareCommaSeparatedIds(subCategoryIds);

            //pass vendors identifiers as comma-delimited string
            string commaSeparatedVendorIds = PrepareCommaSeparatedIds(vendorIds);

            //prepare sp parametors
            var pCategoryId = new DataParameter()
            {
                Name = "Categoryid",
                Value = rootCategoryId,
                DataType = LinqToDB.DataType.Int32
            };

            var pSubCategoryIds = new DataParameter()
            {
                Name = "SubCategoryids",
                Value = commaSeparatedSubCategoryIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pVendorIds = new DataParameter()
            {
                Name = "VendorIds",
                Value = commaSeparatedVendorIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            try
            {
                var result = await _dataProvider.QueryProcAsync<BaseFilterResult>(
                "ApiFilterLoadBaseAndSubCategories",
                pCategoryId,
                pSubCategoryIds,
                pVendorIds);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Get base filter result
        /// </summary>
        /// <param name="rootCategoryId"></param>
        /// <param name="vendorIds"></param>
        /// <param name="subCategoryIds"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<IList<ManufacturerFilterResult>> GetManufacturerFilterResultAsync(
            int rootCategoryId,
            List<int> vendorIds,
            List<int> subCategoryIds = null,
            List<int> manufacturerIds = null)
        {
            //pass sub categories identifiers as comma-delimited string
            string commaSeparatedSubCategoryIds = PrepareCommaSeparatedIds(subCategoryIds);

            //pass vendors identifiers as comma-delimited string
            string commaSeparatedVendorIds = PrepareCommaSeparatedIds(vendorIds);

            //pass manufacturers identifiers as comma-delimited string
            string commaSeparatedManufacturersIds = PrepareCommaSeparatedIds(manufacturerIds);

            //prepare sp parametors
            var pCategoryId = new DataParameter()
            {
                Name = "Categoryid",
                Value = rootCategoryId,
                DataType = LinqToDB.DataType.Int32
            };

            var pSubCategoryIds = new DataParameter()
            {
                Name = "SubCategoryids",
                Value = commaSeparatedSubCategoryIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pVendorIds = new DataParameter()
            {
                Name = "VendorIds",
                Value = commaSeparatedVendorIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pManufactureIds = new DataParameter()
            {
                Name = "ManufactureIds",
                Value = commaSeparatedManufacturersIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            try
            {
                var result = await _dataProvider.QueryProcAsync<ManufacturerFilterResult>(
                "ApiFilterLoadManufacturers",
                pCategoryId,
                pSubCategoryIds,
                pVendorIds,
                pManufactureIds);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

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
        public async Task<IList<VendorFilterResult>> GetVendorFilterResultAsync(
            int rootCategoryId,
            List<int> vendorIds,
            List<int> subCategoryIds = null,
            List<int> manufacturerIds = null,
            List<int> availableVendorIds = null)
        {
            //pass sub categories identifiers as comma-delimited string
            string commaSeparatedSubCategoryIds = PrepareCommaSeparatedIds(subCategoryIds);

            //pass vendors identifiers as comma-delimited string
            string commaSeparatedVendorIds = PrepareCommaSeparatedIds(vendorIds);

            //pass manufacturers identifiers as comma-delimited string
            string commaSeparatedManufacturersIds = PrepareCommaSeparatedIds(manufacturerIds);

            //pass available(geo) vendors identifiers as comma-delimited string
            string commaSeparatedAvailableVendorIds = PrepareCommaSeparatedIds(availableVendorIds);

            //prepare sp parametors
            var pCategoryId = new DataParameter()
            {
                Name = "Categoryid",
                Value = rootCategoryId,
                DataType = LinqToDB.DataType.Int32
            };

            var pSubCategoryIds = new DataParameter()
            {
                Name = "SubCategoryids",
                Value = commaSeparatedSubCategoryIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pVendorIds = new DataParameter()
            {
                Name = "VendorIds",
                Value = commaSeparatedVendorIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pManufactureIds = new DataParameter()
            {
                Name = "ManufactureIds",
                Value = commaSeparatedManufacturersIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pAvailableVendorIds = new DataParameter()
            {
                Name = "AvailableVendorIds",
                Value = commaSeparatedAvailableVendorIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            try
            {
                var result = await _dataProvider.QueryProcAsync<VendorFilterResult>(
                "ApiFilterLoadVendors",
                pCategoryId,
                pSubCategoryIds,
                pVendorIds,
                pManufactureIds,
                pAvailableVendorIds);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

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
        public async Task<IList<SpecificationAttributeOptionFilterResult>> GetSpecificationAttributeOptionFilterResultAsync(
            int rootCategoryId,
            int specificationAttributeId,
            List<int> vendorIds,
            List<int> subCategoryIds = null,
            List<int> manufacturerIds = null,
            List<int> specificationAttOptionIds = null)
        {
            if (specificationAttributeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(specificationAttributeId));

            //pass sub categories identifiers as comma-delimited string
            string commaSeparatedSubCategoryIds = PrepareCommaSeparatedIds(subCategoryIds);

            //pass vendors identifiers as comma-delimited string
            string commaSeparatedVendorIds = PrepareCommaSeparatedIds(vendorIds);

            //pass manufacturers identifiers as comma-delimited string
            string commaSeparatedManufacturersIds = PrepareCommaSeparatedIds(manufacturerIds);

            //pass spec att options identifiers as comma-delimited string
            string commaSeparatedSpecAttOptionIds = PrepareCommaSeparatedIds(specificationAttOptionIds);

            //prepare sp parametors
            var pCategoryId = new DataParameter()
            {
                Name = "Categoryid",
                Value = rootCategoryId,
                DataType = LinqToDB.DataType.Int32
            };

            var pSpecificationAttributeId = new DataParameter()
            {
                Name = "CallFor_SpecificationAttrId",
                Value = specificationAttributeId,
                DataType = LinqToDB.DataType.Int32
            };

            var pSubCategoryIds = new DataParameter()
            {
                Name = "SubCategoryids",
                Value = commaSeparatedSubCategoryIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pVendorIds = new DataParameter()
            {
                Name = "VendorIds",
                Value = commaSeparatedVendorIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pManufactureIds = new DataParameter()
            {
                Name = "ManufactureIds",
                Value = commaSeparatedManufacturersIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            var pSpecAttOptionIds = new DataParameter()
            {
                Name = "SpecificationOptionIds",
                Value = commaSeparatedSpecAttOptionIds,
                DataType = LinqToDB.DataType.NVarChar
            };

            try
            {
                var result = await _dataProvider.QueryProcAsync<SpecificationAttributeOptionFilterResult>(
                "ApiFilterLoadSpecificationOptions",
                pCategoryId,
                pSpecificationAttributeId,
                pSubCategoryIds,
                pVendorIds,
                pManufactureIds,
                pSpecAttOptionIds);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion
    }
}