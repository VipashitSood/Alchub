using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Alchub.ServiceFee;
using Nop.Services.DeliveryFees;
using System.Linq;
using Nop.Services.TipFees;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Represents a order total calculation service
    /// </summary>
    public partial class OrderTotalCalculationService : IOrderTotalCalculationService
    {
        #region Fields
        private readonly IServiceFeeManager _serviceFeeManager;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAddressService _addressService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductService _productService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ITipFeeService _tipFeeService;

        #endregion

        #region Ctor

        public OrderTotalCalculationService(IServiceFeeManager serviceFeeManager,
            CatalogSettings catalogSettings,
            IAddressService addressService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICustomerService customerService,
            IDiscountService discountService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPriceCalculationService priceCalculationService,
            IProductService productService,
            IRewardPointService rewardPointService,
            IShippingPluginManager shippingPluginManager,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            TaxSettings taxSettings,
            IDeliveryFeeService deliveryFeeService,
            ITipFeeService tipFeeService)
        {
            _serviceFeeManager = serviceFeeManager;
            _catalogSettings = catalogSettings;
            _addressService = addressService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _customerService = customerService;
            _discountService = discountService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _orderService = orderService;
            _paymentService = paymentService;
            _priceCalculationService = priceCalculationService;
            _productService = productService;
            _rewardPointService = rewardPointService;
            _shippingPluginManager = shippingPluginManager;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _taxSettings = taxSettings;
            _deliveryFeeService = deliveryFeeService;
            _tipFeeService = tipFeeService;
        }
        #endregion

        #region methods
        /// <summary>
        /// Gets shopping cart total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="useRewardPoints">A value indicating reward points should be used; null to detect current choice of the customer</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shopping cart total;Null if shopping cart total couldn't be calculated now. Applied gift cards. Applied discount amount. Applied discounts. Reward points to redeem. Reward points amount in primary store currency to redeem
        /// </returns>
        public virtual async Task<(decimal? shoppingCartTotal, decimal discountAmount, List<Discount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards, int redeemedRewardPoints, decimal redeemedRewardPointsAmount)> GetShoppingCartTotalAsync(IList<ShoppingCartItem> cart,
            bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true)
        {
            var redeemedRewardPoints = 0;
            var redeemedRewardPointsAmount = decimal.Zero;

            var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
            var store = await _storeContext.GetCurrentStoreAsync();
            var paymentMethodSystemName = string.Empty;

            if (customer != null)
            {
                paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
            }

            //subtotal without tax
            var (_, _, _, subTotalWithDiscountBase, _) = await GetShoppingCartSubTotalAsync(cart, false);
            //subtotal with discount
            var subtotalBase = subTotalWithDiscountBase;

            //shipping without tax
            var shoppingCartShipping = (await GetShoppingCartShippingTotalAsync(cart, false)).shippingTotal;

            //payment method additional fee without tax
            var paymentMethodAdditionalFeeWithoutTax = decimal.Zero;
            if (usePaymentMethodAdditionalFee && !string.IsNullOrEmpty(paymentMethodSystemName))
            {
                var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart,
                    paymentMethodSystemName);
                paymentMethodAdditionalFeeWithoutTax =
                    (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee,
                        false, customer)).price;
            }

            //tax
            var shoppingCartTax = (await GetTaxTotalAsync(cart, usePaymentMethodAdditionalFee)).taxTotal;

            //service fee
            var serviceFee = await _serviceFeeManager.GetServiceFeeAsync(subtotalBase);
            serviceFee = await _priceCalculationService.RoundPriceAsync(serviceFee);

            //slot fee
            var slotFee = await PrepareSlotTotalAsync(cart);
            slotFee = await _priceCalculationService.RoundPriceAsync(slotFee);

            //order total
            var resultTemp = decimal.Zero;
            resultTemp += subtotalBase;
            resultTemp += serviceFee;
            resultTemp += slotFee;

            /*Alchub Start*/
            //Delivery Fee
            var vendorWiseDeliveryFees = await _deliveryFeeService.GetVendorWiseDeliveryFeeAsync(cart);
            resultTemp += vendorWiseDeliveryFees?.Sum(x => x.DeliveryFeeValue) ?? decimal.Zero;

            //Tip fee
            var vendorWiseTipFees = await _tipFeeService.GetVendorWiseTipFeeAsync(cart, subtotalBase);
            resultTemp += vendorWiseTipFees?.Sum(x => x.TipFeeValue) ?? decimal.Zero;
            /*Alchub End*/

            if (shoppingCartShipping.HasValue)
            {
                resultTemp += shoppingCartShipping.Value;
            }

            resultTemp += paymentMethodAdditionalFeeWithoutTax;
            resultTemp += shoppingCartTax;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            //order total discount
            var (discountAmount, appliedDiscounts) = await GetOrderTotalDiscountAsync(customer, resultTemp);

            //sub totals with discount        
            if (resultTemp < discountAmount)
                discountAmount = resultTemp;

            //reduce subtotal
            resultTemp -= discountAmount;

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            //let's apply gift cards now (gift cards that can be used)
            var appliedGiftCards = new List<AppliedGiftCard>();
            resultTemp = await AppliedGiftCardsAsync(cart, appliedGiftCards, customer, resultTemp);

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

            if (!shoppingCartShipping.HasValue)
            {
                //we have errors
                return (null, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount);
            }

            var orderTotal = resultTemp;

            //reward points
            (redeemedRewardPoints, redeemedRewardPointsAmount) = await SetRewardPointsAsync(redeemedRewardPoints, redeemedRewardPointsAmount, useRewardPoints, customer, orderTotal);

            orderTotal -= redeemedRewardPointsAmount;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                orderTotal = await _priceCalculationService.RoundPriceAsync(orderTotal);
            return (orderTotal, discountAmount, appliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount);
        }


        protected virtual async Task<decimal> PrepareSlotTotalAsync(IList<ShoppingCartItem> cart)
        {
            decimal customerSlotFee = 0;
            var slotPrice = cart.GroupBy(x => new { x.SlotId, x.IsPickup,x.SlotPrice }).Select(x => x.Key.SlotPrice).ToList();
            customerSlotFee = slotPrice.Sum();
            return customerSlotFee;
        }
        #endregion
    }
}
