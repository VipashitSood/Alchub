using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Api.Models.ProductsParameters;
using Nop.Plugin.Api.Models.Slots;

namespace Nop.Plugin.Api.Factories
{
    /// <summary>
    /// Represents the interface of the product model factory
    /// </summary>
    public partial interface IProductModelFactory
    {
        Task<IEnumerable<ProductOverviewModel>> PrepareProductOverviewModelsAsync(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false);

        Task<ProductVendorModel> PrepareProductVendorsAsync(Product product, IList<Vendor> availableVendors, Customer customer, int sortBy);

        Task<IList<ProductVendorModel.ProductDetailVendors>> PrepareProductVendorListAsync(Product product, IList<Vendor> availableVendors, Customer customer, bool preLoadFastestSlot = true, bool loadStartTime = true);

        Task<List<BookingSlotModel>> PreparepareSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false);

        Task<List<BookingSlotModel>> PrepareparePickupSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = true);

        Task<List<BookingSlotModel>> PreparepareAdminSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false);

        Task<SlotModel> PrepareTodaySlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false, bool isDelivery = false, bool isAdmin = false);

        /// <summary>
        /// Prepare fstest slot: for product list
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<(string slotTiming, SlotModel slotModel)> PrepareProductFastestSlotAsync(Product product, Customer customer);
    }
}