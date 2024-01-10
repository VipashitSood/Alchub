using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Slots;
using Nop.Core.Domain.Vendors;
using Nop.Web.Alchub.Models.Catalog;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Slots;

namespace Nop.Web.Factories
{

    /// <summary>
    /// Represents the interface of the product model factory
    /// </summary>
    public partial interface IProductModelFactory
    {

        Task<SlotModel> PrepareTodaySlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false, bool isDelivery = false, bool isAdmin = false);

        /// <summary>
        /// Prepare the vendor product price model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product price model
        /// </returns>
        Task<IList<ProductDetailsModel.ProductVendorModel>> PrepareProductVendorsAsync(Product product, IList<Vendor> availableVendors, bool loadStartTime = true, Customer customer = null);

        /// <summary>
        /// Prepare the vendor product price model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product price model
        /// </returns>
        Task<ProductDetailsModel.ProductPriceModel> PrepareVendorProductPriceModelAsync(Product product);

        /// <summary>
        ///  Prepare the Prepareslotlist model
        /// </summary>
        /// <param name="slots"></param>
        /// <param name="startDate"></param>
        /// <returns>A tasl that reprsnts the asynchronous opertion
        /// the task result contains the slot management</returns>
        Task<IEnumerable<dynamic>> PreparepareSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false);

        Task<IEnumerable<dynamic>> PrepareparePickupSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false);

        Task<IEnumerable<dynamic>> PreparepareAdminSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = true);

        /// <summary>
        /// Prepare fastest slot string
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        Task<string> PrepareProductFastestSlotAsync(Product product, Customer customer);

        /// <summary>
        /// prepare product details vendor sorting options.
        /// </summary>
        /// <returns></returns>
        Task<IList<SelectListItem>> PrepareProductDetailsVendorSortingOptions();

        /// <summary>
        /// Prepare the product overview models
        /// </summary>
        /// <param name="products">Collection of products</param>
        /// <param name="preparePictureModel">Whether to prepare the picture model</param>
        /// <param name="productThumbPictureSize">Product thumb picture size (longest side); pass null to use the default value of media settings</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of product overview model
        /// </returns>
        Task<IEnumerable<ProductSearchOverviewModel>> PrepareProductSearchOverviewModelsAsync(IEnumerable<Product> products,
            bool preparePictureModel = true,
            int? productThumbPictureSize = null);
    }
}