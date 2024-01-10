using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the product model factory
    /// </summary>
    public partial interface IProductModelFactory
    {
        Task<ProductListModel> PrepareMasterProductListModelAsync(ProductSearchModel searchModel);

        /// <summary>
        /// Prepare associated product model
        /// </summary>
        /// <param name="associatedProduct">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the associated product model
        /// </returns>
        Task<AssociatedProductModel> PrepareAssociatedProductModelAsync(Product associatedProduct);


        Task<ProductListModel> PrepareVendorMasterProductListModelAsync(ProductSearchModel searchModel);

        Task<ProductListModel> PrepareVendorForMasterProductListModelAsync(ProductSearchModel searchModel);
    }
}