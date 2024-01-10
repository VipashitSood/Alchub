using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Shopping cart service
    /// </summary>
    public partial interface IShoppingCartService
    {
        #region Methods

        /// <summary>
        /// Get Vendor Minimum Order Amount Warnings
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        Task<IList<string>> GetVendorMinimumOrderAmountWarningsAsync(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Get Vendor Minimum Order Amount Warnings
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        Task<IList<string>> GetVendorMinimumOrderAmountWarningsAsync(IList<ShoppingCartItem> cart, Vendor vendor);

        /// <summary>
        /// Check whether shopping cart item is deliverable or not. 
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        Task<(string warning, bool deliverable)> IsItemDeliverable(Product product, Customer customer, ShoppingCartItem shoppingCartItem = null);


        Task<IList<string>> AddToCartAsync(Customer customer, Product product,
            ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true, bool ispickup = false, int masterProductId = 0, int groupedProductId = 0, bool ismigrate = false);


        Task<IList<string>> UpdateShoppingCartItemAsync(Customer customer,
          int shoppingCartItemId, string attributesXml,
          decimal customerEnteredPrice,
          DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
          int quantity = 1, bool resetCheckoutData = true, bool ispickup = false, int masterProductId = 0, int groupedProductId = 0);


        Task<IList<string>> GetShoppingCartItemWarningsAsync(Customer customer, ShoppingCartType shoppingCartType,
       Product product, int storeId,
       string attributesXml, decimal customerEnteredPrice,
       DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
       int quantity = 1, bool addRequiredProducts = true, int shoppingCartItemId = 0,
       bool getStandardWarnings = true, bool getAttributesWarnings = true,
       bool getGiftCardWarnings = true, bool getRequiredProductWarnings = true,
       bool getRentalWarnings = true, bool ismigrate = false);

        #endregion
    }
}