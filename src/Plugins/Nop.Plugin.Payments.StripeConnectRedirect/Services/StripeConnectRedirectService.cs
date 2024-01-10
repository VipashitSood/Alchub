using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Payments.StripeConnectRedirect.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.DeliveryFees;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.TipFees;
using Nop.Services.Vendors;
using Stripe;
using Stripe.Checkout;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Services
{
    /// <summary>
    /// Represents the Stripe Connect Redirect Service
    /// </summary>
    public partial class StripeConnectRedirectService : IStripeConnectRedirectService
    {
        #region Fields

        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderItemRefundService _orderItemRefundService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<StripeVendorConnect> _stripeVendorConnectRepository;
        private readonly IRepository<StripeOrder> _stripeOrderRepository;
        private readonly IRepository<StripeTransfer> _stripeTransferRepository;
        private readonly ITipFeeService _tipFeeService;
        private readonly IVendorService _vendorService;
        private readonly IAddressService _addressService;
        private readonly IWebHelper _webHelper;
        private readonly StripeConnectRedirectPaymentSettings _stripeConnectRedirectPaymentSettings;
        private readonly IPriceFormatter _priceFormatter;

        #endregion Fields

        #region Ctor

        public StripeConnectRedirectService(
            IDeliveryFeeService deliveryFeeService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderItemRefundService orderItemRefundService,
            IOrderService orderService,
            IProductService productService,
            IRepository<Vendor> vendorRepository,
            IRepository<StripeVendorConnect> stripeVendorConnectRepository,
            IRepository<StripeOrder> stripeOrderRepository,
            IRepository<StripeTransfer> stripeTransferRepository,
            ITipFeeService tipFeeService,
            IVendorService vendorService,
            IAddressService addressService,
            IWebHelper webHelper,
            StripeConnectRedirectPaymentSettings stripeConnectRedirectPaymentSettings,
            IPriceFormatter priceFormatter)
        {
            _deliveryFeeService = deliveryFeeService;
            _localizationService = localizationService;
            _logger = logger;
            _orderItemRefundService = orderItemRefundService;
            _orderService = orderService;
            _productService = productService;
            _vendorRepository = vendorRepository;
            _stripeVendorConnectRepository = stripeVendorConnectRepository;
            _stripeOrderRepository = stripeOrderRepository;
            _stripeTransferRepository = stripeTransferRepository;
            _tipFeeService = tipFeeService;
            _vendorService = vendorService;
            _addressService = addressService;
            _webHelper = webHelper;
            _stripeConnectRedirectPaymentSettings = stripeConnectRedirectPaymentSettings;
            _priceFormatter = priceFormatter;
        }

        #endregion Ctor

        #region Utilities

        /// <summary>
        /// IsConfigured
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public virtual bool IsConfigured(StripeConnectRedirectPaymentSettings settings)
        {
            return !string.IsNullOrEmpty(settings?.PublishableApiKey) && !string.IsNullOrEmpty(settings?.SecretApiKey) && !string.IsNullOrEmpty(settings?.ClientId);
        }

        #endregion Utilities

        #region Methods

        #region Stripe Vendor Connect

        /// <summary>
        /// Insert a Stripe Vendor Connect
        /// </summary>
        /// <param name="stripeVendorConnect"></param>
        public virtual async Task InsertStripeVendorConnectAsync(StripeVendorConnect stripeVendorConnect)
        {
            if (stripeVendorConnect == null)
                throw new ArgumentNullException(nameof(stripeVendorConnect));

            await _stripeVendorConnectRepository.InsertAsync(stripeVendorConnect);
        }

        /// <summary>
        /// Update a Stripe Vendor Connect
        /// </summary>
        /// <param name="stripeVendorConnect"></param>
        public virtual async Task UpdateStripeVendorConnectAsync(StripeVendorConnect stripeVendorConnect)
        {
            if (stripeVendorConnect == null)
                throw new ArgumentNullException(nameof(stripeVendorConnect));

            await _stripeVendorConnectRepository.UpdateAsync(stripeVendorConnect);
        }

        /// <summary>
        /// Delete a Stripe Vendor Connect
        /// </summary>
        /// <param name="stripeVendorConnect"></param>
        public virtual async Task DeleteStripeVendorConnectAsync(StripeVendorConnect stripeVendorConnect)
        {
            if (stripeVendorConnect == null)
                throw new ArgumentNullException(nameof(stripeVendorConnect));

            await _stripeVendorConnectRepository.DeleteAsync(stripeVendorConnect);
        }

        /// <summary>
        /// Search Stripe Vendor Connect
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<StripeVendor>> SearchStripeVendorConnectAsync(
           int pageIndex = 0,
           int pageSize = int.MaxValue)
        {
            string stringResource = await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.Configure.Vendor.Account.NotLinkedYet");

            var query = from v in _vendorRepository.Table
                        join vcws in _stripeVendorConnectRepository.Table on
                        v.Id equals vcws.VendorId into n
                        from vcws in n.DefaultIfEmpty()
                        where v.Deleted == false && v.Active == true
                        select new StripeVendor
                        {
                            Id = v.Id,
                            Name = v.Name,
                            Email = v.Email,
                            Account = (vcws == null || string.IsNullOrEmpty(vcws.Account)) ? stringResource : vcws.Account,
                            AdminDeliveryCommissionPercentage = vcws == null ? 0 : vcws.AdminDeliveryCommissionPercentage,
                            AdminPickupCommissionPercentage = vcws == null ? 0 : vcws.AdminPickupCommissionPercentage,
                            IsVerified = vcws.IsVerified
                        };

            var allVendors = new PagedList<StripeVendor>(query.ToList(), pageIndex, pageSize);

            return allVendors;
        }

        /// <summary>
        /// Get Stripe Vendor Connect By Vendor Id
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Vendor Connect With Stripe</returns>
        public virtual async Task<StripeVendorConnect> GetStripeVendorConnectByVendorIdAsync(int vendorId)
        {
            if (vendorId == 0)
                return null;

            return await _stripeVendorConnectRepository.Table.FirstOrDefaultAsync(x => x.VendorId == vendorId);
        }

        /// <summary>
        /// Get Stripe Vendor Connects For Verifying
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IList<StripeVendorConnect>> GetStripeVendorConnectsForVerifyingAsync()
        {
            var query = _stripeVendorConnectRepository.Table;
            query = query.Where(x => !string.IsNullOrEmpty(x.Account) && !x.IsVerified);
            return await query.ToListAsync();
        }

        #endregion Stripe Vendor Connect

        #region Stripe Order

        /// <summary>
        /// Insert a Stripe Order
        /// </summary>
        /// <param name="stripeOrder">Stripe Order</param>
        public virtual async Task InsertStripeOrderAsync(StripeOrder stripeOrder)
        {
            if (stripeOrder == null)
                throw new ArgumentNullException(nameof(stripeOrder));

            await _stripeOrderRepository.InsertAsync(stripeOrder);
        }

        /// <summary>
        /// Update a Stripe Order
        /// </summary>
        /// <param name="stripeOrder">Stripe Order</param>
        public virtual async Task UpdateStripeOrderAsync(StripeOrder stripeOrder)
        {
            if (stripeOrder == null)
                throw new ArgumentNullException(nameof(stripeOrder));

            await _stripeOrderRepository.UpdateAsync(stripeOrder);
        }

        /// <summary>
        /// Delete a Stripe Order
        /// </summary>
        /// <param name="stripeOrder">Stripe Order</param>
        public virtual async Task DeleteStripeOrderAsync(StripeOrder stripeOrder)
        {
            await _stripeOrderRepository.UpdateAsync(stripeOrder);
        }

        /// <summary>
        ///Get Stripe Order By Id
        /// </summary>
        /// <param name="stripeOrderId"></param>
        /// <returns></returns>
        public virtual async Task<StripeOrder> GetStripeOrderByIdAsync(int stripeOrderId)
        {
            return await _stripeOrderRepository.GetByIdAsync(stripeOrderId);
        }

        /// <summary>
        /// Get Stripe Order By Session Id
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public virtual async Task<StripeOrder> GetStripeOrderBySessionIdAsync(string sessionId)
        {
            return await _stripeOrderRepository.Table.Where(x => x.SessionId == sessionId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get Stripe Order By Payment Intent Id
        /// </summary>
        /// <param name="paymentIntentId"></param>
        /// <returns></returns>
        public virtual async Task<StripeOrder> GetStripeOrderByPaymentIntentIdAsync(string paymentIntentId)
        {
            return await _stripeOrderRepository.Table.Where(x => x.PaymentIntentId == paymentIntentId).FirstOrDefaultAsync();
        }

        /// <summary>
        ///Get Stripe Order By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual async Task<StripeOrder> GetStripeOrderByOrderIdAsync(int orderId)
        {
            return await _stripeOrderRepository.Table.FirstOrDefaultAsync(x => x.OrderId == orderId);
        }

        #endregion Stripe Order

        #region Stripe Transfer

        /// <summary>
        /// Insert a Stripe Transfer
        /// </summary>
        /// <param name="stripeTransfer"></param>
        /// <returns></returns>
        public virtual async Task InsertStripeTransferAsync(StripeTransfer stripeTransfer)
        {
            if (stripeTransfer == null)
                throw new ArgumentNullException(nameof(stripeTransfer));

            await _stripeTransferRepository.InsertAsync(stripeTransfer);
        }

        /// <summary>
        /// Insert a list of Stripe Transfers
        /// </summary>
        /// <param name="stripeTransfers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task InsertStripeTransfersAsync(IList<StripeTransfer> stripeTransfers)
        {
            if (stripeTransfers == null)
                throw new ArgumentNullException(nameof(stripeTransfers));

            await _stripeTransferRepository.InsertAsync(stripeTransfers);
        }

        /// <summary>
        /// Update a Stripe Transfer
        /// </summary>
        /// <param name="stripeTransfer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateStripeTransferAsync(StripeTransfer stripeTransfer)
        {
            if (stripeTransfer == null)
                throw new ArgumentNullException(nameof(stripeTransfer));

            await _stripeTransferRepository.UpdateAsync(stripeTransfer);
        }

        /// <summary>
        /// Delete a Stripe Transfer
        /// </summary>
        /// <param name="stripeTransfer"></param>
        /// <returns></returns>
        public virtual async Task DeleteStripeTransferAsync(StripeTransfer stripeTransfer)
        {
            if (stripeTransfer == null)
                throw new ArgumentNullException(nameof(stripeTransfer));

            await _stripeTransferRepository.DeleteAsync(stripeTransfer);
        }

        /// <summary>
        /// Get a Stripe Transfer by Id
        /// </summary>
        /// <param name="stripeTransferId"></param>
        /// <returns></returns>
        public virtual async Task<StripeTransfer> GetStripeTransferByIdAsync(int stripeTransferId)
        {
            return await _stripeTransferRepository.GetByIdAsync(stripeTransferId);
        }

        /// <summary>
        /// Get Stripe Transfers By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual async Task<IList<StripeTransfer>> GetStripeTransfersByOrderIdAsync(int orderId)
        {
            return await _stripeTransferRepository.Table.Where(x => x.OrderId == orderId).ToListAsync();
        }

        /// <summary>
        /// Get Stripe Transfers By Order Id and Vendor Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        public virtual async Task<IList<StripeTransfer>> GetStripeTransfersByOrderIdVendorIdAsync(int orderId, int vendorId)
        {
            return await _stripeTransferRepository.Table.Where(x => x.OrderId == orderId && x.VendorId == vendorId).ToListAsync();
        }

        #endregion Stripe Transfer

        #region Order

        /// <summary>
        /// Stripe Order Payment Authorized
        /// </summary>
        /// <param name="paymentIntent"></param>
        /// <returns></returns>
        public virtual async Task StripeOrderPaymentAuthorizedAsync(PaymentIntent paymentIntent)
        {
            StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;
            try
            {
                if (paymentIntent == null)
                {
                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Authorized - PaymentIntent not found: {0}", paymentIntent.Id));
                    return;
                }

                var sessionService = new SessionService();

                var stripeOrder = await GetStripeOrderByPaymentIntentIdAsync(paymentIntent.Id);
                if (stripeOrder == null)
                {
                    var options = new SessionListOptions { PaymentIntent = paymentIntent.Id };
                    StripeList<Session> sessions = sessionService.List(options);

                    if (sessions != null && sessions.Data != null && sessions.Data.Count > 0)
                        stripeOrder = await GetStripeOrderBySessionIdAsync(sessions.Data.FirstOrDefault().Id);

                    if (stripeOrder == null)
                    {
                        await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Authorized - StripeOrder not found by payment intent id: {0}", paymentIntent.Id));
                        return;
                    }
                }

                var order = await _orderService.GetOrderByIdAsync(stripeOrder.OrderId);
                if (order == null)
                {
                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Authorized - Order not found by StripeOrder id: {0}", stripeOrder.Id));
                    return;
                }


                Session session = await sessionService.GetAsync(stripeOrder.SessionId);
                if (session == null)
                {
                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Authorized - Session not found by StripeOrder id: {0}", stripeOrder.Id));
                    return;
                }

                var _orderProcessingService = EngineContext.Current.Resolve<IOrderProcessingService>();

                if (!_orderProcessingService.CanMarkOrderAsAuthorized(order))
                {
                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Authorized - Order {0} cannot be marked as Authorized.", order.Id));
                    return;
                }

                order.AuthorizationTransactionId = session.Id;
                order.AuthorizationTransactionResult = session.Status;
                order.CaptureTransactionId = session.PaymentIntentId;
                order.CaptureTransactionResult = session.PaymentStatus;
                await _orderService.UpdateOrderAsync(order);

                //Mark Order as Authorized
                await _orderProcessingService.MarkAsAuthorizedAsync(order: order, performPaidAction: true);

                //Update Stripe details here
                stripeOrder.SessionStatus = session.Status;
                stripeOrder.PaymentStatus = session.PaymentStatus;
                await UpdateStripeOrderAsync(stripeOrder);

                //order note
                var sb = new StringBuilder();
                sb.AppendLine("Stripe payment authorized successfully.");
                sb.AppendLine("Payment Intent: " + session.PaymentIntentId);

                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = Convert.ToString(sb),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                return;
            }
            catch (Exception exc)
            {
                await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Authorized - Error occured for payment intent: {0}", paymentIntent?.Id), exception: exc);
                return;
            }
        }

        /// <summary>
        /// Stripe Order Payment Fail
        /// </summary>
        /// <param name="paymentIntent"></param>
        /// <returns></returns>
        public virtual async Task StripeOrderPaymentFailAsync(PaymentIntent paymentIntent)
        {
            StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;
            try
            {
                if (paymentIntent == null)
                {
                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Fail - PaymentIntent not found: {0}", paymentIntent.Id));
                    return;
                }

                var stripeOrder = await GetStripeOrderByPaymentIntentIdAsync(paymentIntent.Id);
                if (stripeOrder == null)
                {
                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Fail - StripeOrder not found by payment intent id: {0}", paymentIntent.Id));
                    return;
                }

                var order = await _orderService.GetOrderByIdAsync(stripeOrder.OrderId);
                if (order == null)
                {
                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Fail - Order not found by StripeOrder id: {0}", stripeOrder.Id));
                    return;
                }

                var sessionService = new SessionService();
                Session session = await sessionService.GetAsync(stripeOrder.SessionId);
                if (session == null)
                {
                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Fail - Session not found by StripeOrder id: {0}", stripeOrder.Id));
                    return;
                }

                //order note
                var sb = new StringBuilder();
                sb.AppendLine("Stripe payment failed.");

                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = Convert.ToString(sb),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                stripeOrder.SessionStatus = session.Status;
                stripeOrder.PaymentIntentId = session.PaymentIntentId;
                stripeOrder.PaymentStatus = session.PaymentStatus;
                await UpdateStripeOrderAsync(stripeOrder);

                return;
            }
            catch (Exception exc)
            {
                await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Fail - Error occured for payment intent: {0}", paymentIntent?.Id), exception: exc);
                return;
            }
        }

        public partial class VendorSlotFee : BaseEntity
        {
            /// <summary>
            /// Gets or sets the Vendor Id
            /// </summary>
            public int VendorId { get; set; }

            /// <summary>
            /// Gets or sets the Slot Id
            /// </summary>
            public int SlotId { get; set; }

            /// <summary>
            /// Gets or sets the Slot Fee
            /// </summary>
            public decimal SlotFee { get; set; }
        }

        /// <summary>
        /// Save Order Transfers Async
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public virtual async Task SaveOrderTransfersAsync(Order order)
        {
            if (order == null)
                return;

            var orderItems = await _orderService.GetOrderItemsAsync(orderId: order.Id);
            if (orderItems == null || orderItems.Count <= 0)
                return;

            var stripeTransfers = new List<StripeTransfer>();
            var vendorSlotFee = new List<VendorSlotFee>();

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);

                if (vendor != null)
                {
                    decimal deliverySubtotalInclTax = decimal.Zero;
                    decimal deliverySubtotalExclTax = decimal.Zero;
                    decimal pickupSubtotalInclTax = decimal.Zero;
                    decimal pickupSubtotalExclTax = decimal.Zero;

                    if (orderItem.InPickup)
                    {
                        pickupSubtotalInclTax = orderItem.PriceInclTax;
                        pickupSubtotalExclTax = orderItem.PriceExclTax;
                    }
                    else
                    {
                        deliverySubtotalInclTax = orderItem.PriceInclTax;
                        deliverySubtotalExclTax = orderItem.PriceExclTax;
                    }

                    if (stripeTransfers.Any(x => x.OrderId == order.Id && x.VendorId == vendor.Id))
                    {
                        stripeTransfers.Where(x => x.OrderId == order.Id && x.VendorId == vendor.Id).ToList().ForEach(y =>
                        {
                            y.DeliverySubtotalInclTax += deliverySubtotalInclTax;
                            y.DeliverySubtotalExclTax += deliverySubtotalExclTax;
                            y.PickupSubtotalInclTax += pickupSubtotalInclTax;
                            y.PickupSubtotalExclTax += pickupSubtotalExclTax;
                        });
                    }
                    else
                    {
                        stripeTransfers.Add(new StripeTransfer
                        {
                            OrderId = order.Id,
                            VendorId = vendor.Id,
                            DeliverySubtotalInclTax = deliverySubtotalInclTax,
                            DeliverySubtotalExclTax = deliverySubtotalExclTax,
                            PickupSubtotalInclTax = pickupSubtotalInclTax,
                            PickupSubtotalExclTax = pickupSubtotalExclTax
                        });
                    }
                }

                //Vendor Order Items
                if (vendor != null && vendor.ManageDelivery)
                {
                    if (!vendorSlotFee.Any(x => x.VendorId == vendor.Id && x.SlotId == orderItem.SlotId))
                    {
                        vendorSlotFee.Add(new VendorSlotFee
                        {
                            VendorId = vendor.Id,
                            SlotId = orderItem.SlotId,
                            SlotFee = orderItem.SlotPrice
                        });
                    }
                }
            }

            //Delivery Fee
            var vendorWiseDeliveryFees = await _deliveryFeeService.GetVendorWiseOrderDeliveryFeesByOrderIdAsync(order.Id);
            if (vendorWiseDeliveryFees != null && vendorWiseDeliveryFees.Count > 0)
            {
                foreach (var vendorWiseDeliveryFee in vendorWiseDeliveryFees.Where(x => x.VendorId != 0).ToList())
                {
                    if (stripeTransfers.Any(x => x.OrderId == order.Id && x.VendorId == vendorWiseDeliveryFee.VendorId))
                    {
                        stripeTransfers.Where(x => x.OrderId == order.Id && x.VendorId == vendorWiseDeliveryFee.VendorId).ToList().ForEach(y =>
                        { y.DeliveryFee += vendorWiseDeliveryFee.DeliveryFeeValue; });
                    }
                    else
                    {
                        stripeTransfers.Add(new StripeTransfer
                        {
                            OrderId = order.Id,
                            VendorId = vendorWiseDeliveryFee.VendorId,
                            DeliveryFee = vendorWiseDeliveryFee.DeliveryFeeValue
                        });
                    }
                }
            }

            //Tip Fee
            var vendorWiseTipFees = await _tipFeeService.GetVendorWiseOrderTipFeesByOrderIdAsync(order.Id);
            if (vendorWiseTipFees != null && vendorWiseTipFees.Count > 0)
            {
                foreach (var vendorWiseTipFee in vendorWiseTipFees.Where(x => x.VendorId != 0).ToList())
                {
                    if (stripeTransfers.Any(x => x.OrderId == order.Id && x.VendorId == vendorWiseTipFee.VendorId))
                    {
                        stripeTransfers.Where(x => x.OrderId == order.Id && x.VendorId == vendorWiseTipFee.VendorId).ToList().ForEach(y =>
                        { y.TipFee += vendorWiseTipFee.TipFeeValue; });
                    }
                    else
                    {
                        stripeTransfers.Add(new StripeTransfer
                        {
                            OrderId = order.Id,
                            VendorId = vendorWiseTipFee.VendorId,
                            TipFee = vendorWiseTipFee.TipFeeValue
                        });
                    }
                }
            }

            foreach (var stripeTransfer in stripeTransfers)
            {
                stripeTransfer.IsTransferred = false;
                stripeTransfer.CreatedOnUtc = DateTime.UtcNow;

                //vendor Stripe Accounts
                var stripeVendorConnect = await GetStripeVendorConnectByVendorIdAsync(stripeTransfer.VendorId);
                if (stripeVendorConnect != null && stripeVendorConnect.IsVerified)
                {
                    stripeTransfer.VendorAccount = stripeVendorConnect.Account;
                }

                //Commission calculation
                stripeTransfer.AdminDeliveryCommissionPercentage = stripeVendorConnect?.AdminDeliveryCommissionPercentage ?? decimal.Zero;
                stripeTransfer.AdminPickupCommissionPercentage = stripeVendorConnect?.AdminPickupCommissionPercentage ?? decimal.Zero;
                stripeTransfer.AdminDeliveryCommission = (stripeTransfer.DeliverySubtotalExclTax * stripeVendorConnect?.AdminDeliveryCommissionPercentage ?? decimal.Zero) / 100;
                stripeTransfer.AdminPickupCommission = (stripeTransfer.PickupSubtotalExclTax * stripeVendorConnect?.AdminPickupCommissionPercentage ?? decimal.Zero) / 100;

                //Slot fee
                if (stripeTransfer.VendorId > 0)
                    stripeTransfer.SlotFee = vendorSlotFee.Where(x => x.VendorId == stripeTransfer.VendorId).Distinct().Sum(y => y.SlotFee);

                //Service fee
                stripeTransfer.ServiceFee = decimal.Zero;   //Since service fee goes to Admin it will always be zero

                //Total Amount Calculation
                stripeTransfer.TotalAmount
                    = stripeTransfer.DeliverySubtotalInclTax
                    + stripeTransfer.PickupSubtotalInclTax
                    - stripeTransfer.AdminDeliveryCommission
                    - stripeTransfer.AdminPickupCommission
                    + stripeTransfer.ServiceFee
                    + stripeTransfer.DeliveryFee
                    + stripeTransfer.SlotFee
                    + stripeTransfer.TipFee;
            }

            //Insert information
            await InsertStripeTransfersAsync(stripeTransfers);
        }

        #endregion Order

        #region Capture/Release Payment

        /// <summary>
        /// Capture the Payment
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual async Task<bool> CapturePaymentAsync(int orderId)
        {
            StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: CapturePayment Order not found for Order Id: {0}", orderId));
                    return false;
                }

                if (order.PaymentMethodSystemName != StripeConnectRedirectPaymentDefaults.SystemName)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: CapturePayment PaymentMethodSystemName is not StripeConnectRedirect for Order Id: {0}", orderId));
                    return false;
                }

                var stripeOrder = await GetStripeOrderByOrderIdAsync(orderId);
                if (stripeOrder == null)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: CapturePayment Stripe Order not found for Order Id: {0}", orderId));
                    return false;
                }

                var sessionService = new SessionService();
                Session session = await sessionService.GetAsync(stripeOrder.SessionId);
                if (session == null)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: CapturePayment Session not found for Stripe Order Id: {0}", stripeOrder.Id));
                    return false;
                }

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = paymentIntentService.Get(session.PaymentIntentId);
                if (paymentIntent == null)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: CapturePayment Payment Intent not found for Session Id: {0}", session.Id));
                    return false;
                }

                var currency = order.CustomerCurrencyCode.ToLower();
                var refunds = (await _orderItemRefundService.GetOrderItemRefundByOrderIdAsync(orderId: stripeOrder.OrderId)).ToList();

                var totalRefund = refunds?.Sum(x => x.TotalAmount) ?? decimal.Zero;
                var totalAmount = Math.Round(stripeOrder.OrderAmount - totalRefund, 2);

                long? amountToCapture = Convert.ToInt64(!Array.Exists<string>(StripeConnectRedirectPaymentDefaults.ZeroDecimal, new Predicate<string>((string z) => z == currency)) ?
                        (int)Math.Ceiling(totalAmount * new decimal(100)) : (int)Math.Ceiling(totalAmount));

                if (amountToCapture <= 0)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: CapturePayment Amount to capture is less than or equal to Zero for Order Id: {0}", orderId));
                    return false;
                }

                var options = new PaymentIntentCaptureOptions
                {
                    AmountToCapture = amountToCapture
                };
                var capture = paymentIntentService.Capture(paymentIntent.Id, options);

                stripeOrder.SessionStatus = session.Status;
                stripeOrder.PaymentIntentId = session.PaymentIntentId;
                stripeOrder.PaymentStatus = session.PaymentStatus;
                await UpdateStripeOrderAsync(stripeOrder);

                //Add order information
                order.AuthorizationTransactionId = session.Id;
                order.AuthorizationTransactionResult = session.Status;
                order.CaptureTransactionId = session.PaymentIntentId;
                order.CaptureTransactionResult = session.PaymentStatus;

                //Add refunded amount
                order.RefundedAmount = order.RefundedAmount + totalRefund;

                //Update Order
                await _orderService.UpdateOrderAsync(order);

                var _orderProcessingService = EngineContext.Current.Resolve<IOrderProcessingService>();

                //Mark order as Paid
                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    await _orderProcessingService.MarkOrderAsPaidAsync(order: order, performPaidAction: false);

                //order note
                var amountReceived = !Array.Exists<string>(StripeConnectRedirectPaymentDefaults.ZeroDecimal, new Predicate<string>((string z) => z == currency)) ?
                    (capture.AmountReceived / new decimal(100)) : capture.AmountReceived;

                //formate amount
                var amountReceivedStr = await _priceFormatter.FormatPriceAsync(amountReceived);
                var totalRefundStr = await _priceFormatter.FormatPriceAsync(totalRefund);

                var sb = new StringBuilder();
                sb.AppendLine("Stripe payment captured successfully.");
                sb.AppendLine("Session: " + session.Id);
                sb.AppendLine("Session Status: " + session.Status);
                sb.AppendLine("Payment Status: " + session.PaymentStatus);
                sb.AppendLine("Amount Captured: " + amountReceivedStr);
                sb.AppendLine("Payment Intent: " + session.PaymentIntentId);
                sb.AppendLine("Payment Intent Status: " + capture.Status);

                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = Convert.ToString(sb),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                await _orderService.UpdateOrderAsync(order);

                #region Vendor Payment

                var stripeTransfers = await GetStripeTransfersByOrderIdAsync(order.Id);

                var chargeId = paymentIntent?.Charges?.FirstOrDefault()?.Id;

                if (stripeTransfers != null && stripeTransfers.Count > 0)
                {
                    var descriptionPrefix = await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.ProductNamePrefix");
                    var transferService = new TransferService();
                    foreach (var stripeTransfer in stripeTransfers)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(stripeTransfer.VendorAccount) && !stripeTransfer.IsTransferred && stripeTransfer.TotalAmount > decimal.Zero)
                            {
                                var vendorRefunds = refunds.Where(x => x.VendorId == stripeTransfer.VendorId && !x.IsRefunded).ToList();

                                var totalVendorRefund = vendorRefunds?.Sum(x => x.TotalAmount) ?? decimal.Zero;
                                var adminCommission = vendorRefunds?.Sum(x => x.AdminCommission) ?? decimal.Zero;

                                var vendorAmount = Math.Round((stripeTransfer.TotalAmount - totalVendorRefund + adminCommission), 2);

                                var amount = !Array.Exists<string>(StripeConnectRedirectPaymentDefaults.ZeroDecimal, new Predicate<string>((string z) => z == currency)) ?
                                    (int)Math.Ceiling(vendorAmount * new decimal(100)) : (int)Math.Ceiling(vendorAmount);

                                if (amount > 0)
                                {
                                    var transferCreateOptions = new TransferCreateOptions
                                    {
                                        Amount = amount,
                                        Currency = currency,
                                        Destination = stripeTransfer.VendorAccount,
                                        TransferGroup = Convert.ToString(order.OrderGuid),
                                        Description = $"{descriptionPrefix}{order.Id}",
                                        Metadata = new Dictionary<string, string>
                                {
                                    { "OrderId", Convert.ToString(order.Id) },
                                    { "OrderGuid", Convert.ToString(order.OrderGuid) }
                                }
                                    };

                                    //Adding Source Transaction
                                    if (!string.IsNullOrEmpty(chargeId))
                                        transferCreateOptions.SourceTransaction = chargeId;

                                    var transfer = transferService.Create(transferCreateOptions);

                                    stripeTransfer.TransferId = transfer.Id;
                                    stripeTransfer.IsTransferred = true;
                                    await UpdateStripeTransferAsync(stripeTransfer);

                                    //order note
                                    var transferAmount = !Array.Exists<string>(StripeConnectRedirectPaymentDefaults.ZeroDecimal, new Predicate<string>((string z) => z == currency)) ?
                                        (transfer.Amount / new decimal(100)) : transfer.Amount;

                                    var sbVendor = new StringBuilder();
                                    sbVendor.AppendLine("Stripe vendor payment transfered successfully.");
                                    sbVendor.AppendLine("Vendor Id: " + stripeTransfer.VendorId);
                                    sbVendor.AppendLine("Destination: " + transfer.Destination);
                                    sbVendor.AppendLine("Session: " + session.Id);
                                    sbVendor.AppendLine("Transfer: " + transfer.Id);
                                    sbVendor.AppendLine("Amount: " + transferAmount);

                                    await _orderService.InsertOrderNoteAsync(new OrderNote
                                    {
                                        OrderId = order.Id,
                                        Note = Convert.ToString(sbVendor),
                                        DisplayToCustomer = false,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });

                                    await _orderService.UpdateOrderAsync(order);

                                    foreach (var refund in vendorRefunds)
                                    {
                                        refund.IsRefunded = true;
                                        await _orderItemRefundService.UpdateOrderItemRefundAsync(refund);
                                    }
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: Stripe vendor payment transfered failed for OrderId: {0}", orderId), exception: exc);

                            var sbError = new StringBuilder();
                            sbError.AppendLine("Stripe vendor payment transfered failed. Check log for more details.");
                            sbError.AppendLine("Vendor Id: " + stripeTransfer.VendorId);
                            sbError.AppendLine("Error: " + exc.Message);

                            await _orderService.InsertOrderNoteAsync(new OrderNote
                            {
                                OrderId = order.Id,
                                Note = Convert.ToString(sbError),
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }
                }

                #endregion Vendor Payment

                #region Email Notification

                try
                {
                    var _stripeWorkflowMessageService = EngineContext.Current.Resolve<IStripeWorkflowMessageService>();
                    await _stripeWorkflowMessageService.SendCapturePaymentCustomerNotificationAsync(
                        order: order,
                        amountCaptured: amountReceivedStr,
                        amountReleased: totalRefundStr,
                        languageId: order.CustomerLanguageId);
                }
                catch (Exception exc)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: CapturePayment Email sending failed for Order Id: {0}", orderId), exception: exc);
                }

                #endregion Email Notification

                return true;
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: CapturePayment Error occured for Order Id: {0}", orderId), exception: exc);
                return false;
            }
        }

        /// <summary>
        /// Release the Payment
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual async Task<bool> ReleasePaymentAsync(int orderId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;

                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: ReleasePayment Order not found for Order Id: {0}", orderId));
                    return false;
                }

                if (order.PaymentMethodSystemName != StripeConnectRedirectPaymentDefaults.SystemName)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: ReleasePayment PaymentMethodSystemName is not StripeConnectRedirect for Order Id: {0}", orderId));
                    return false;
                }

                var stripeOrder = await GetStripeOrderByOrderIdAsync(orderId);
                if (stripeOrder == null)
                {
                    await _logger.ErrorAsync(String.Format("Stripe Connect Redirect: ReleasePayment Stripe Order not found for Order Id: {0}", orderId));
                    return false;
                }

                var sessionService = new SessionService();
                Session session = await sessionService.GetAsync(stripeOrder.SessionId);
                if (session == null)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: ReleasePayment Session not found for Stripe Order Id: {0}", stripeOrder.Id));
                    return false;
                }

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = paymentIntentService.Get(session.PaymentIntentId);
                if (paymentIntent == null)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: ReleasePayment Payment Intent not found for Session Id: {0}", session.Id));
                    return false;
                }

                var cancelledPaymentIntent = paymentIntentService.Cancel(paymentIntent.Id);

                //Add refunded amount
                order.RefundedAmount = order.OrderTotal;

                //Update Order
                await _orderService.UpdateOrderAsync(order);

                //order note
                var sb = new StringBuilder();
                sb.AppendLine("Stripe payment released successfully.");
                sb.AppendLine("Session: " + session.Id);
                sb.AppendLine("Session Payment Status: " + session.PaymentStatus);
                sb.AppendLine("Payment Intent: " + paymentIntent.Id);
                sb.AppendLine("Payment Intent Status: " + cancelledPaymentIntent.Status);

                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = Convert.ToString(sb),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                #region Email Notification

                try
                {
                    var _stripeWorkflowMessageService = EngineContext.Current.Resolve<IStripeWorkflowMessageService>();
                    await _stripeWorkflowMessageService.SendReleasePaymentCustomerNotificationAsync(
                        order: order,
                        amountCaptured: await _priceFormatter.FormatPriceAsync(decimal.Zero),
                        amountReleased: await _priceFormatter.FormatPriceAsync(order.OrderTotal),
                        languageId: order.CustomerLanguageId);
                }
                catch (Exception exc)
                {
                    await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: ReleasePayment Email sending failed."), exception: exc);
                }

                #endregion Email Notification

                return true;
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: ReleasePayment Error occured for Order Id: {0}", orderId), exception: exc);
                return false;
            }
        }

        #endregion Capture/Release Payment

        #region Post process payment

        /// <summary>
        /// post process payment & get payment redirect url
        /// </summary>
        /// <param name="order"></param>
        /// <param name="requestFromApi"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<string> PostProcessPaymentAndGetRedirectUrl(Order order, bool requestFromApi = false)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;

            var currency = order.CustomerCurrencyCode.ToLower();
            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var productIdPrefix = await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.ProductIdPrefix");
            var productNamePrefix = await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.ProductNamePrefix");

            //round order total
            var roundedOrderTotal = Math.Round(order.OrderTotal, 2);

            var orderTotal = !Array.Exists<string>(StripeConnectRedirectPaymentDefaults.ZeroDecimal, new Predicate<string>((string z) => z == currency)) ?
                    (int)Math.Ceiling(roundedOrderTotal * new decimal(100)) : (int)Math.Ceiling(roundedOrderTotal);

            //Stripe Product
            var productCreateOptions = new ProductCreateOptions
            {
                Id = $"{productIdPrefix}{order.OrderGuid}",
                Name = $"{productNamePrefix}{order.Id}"
            };
            var productService = new Stripe.ProductService();
            var product = productService.Create(productCreateOptions);

            //Stripe Price
            var priceCreateOptions = new PriceCreateOptions
            {
                Product = product.Id,
                UnitAmount = orderTotal,
                Currency = currency
            };
            var priceService = new PriceService();
            var price = priceService.Create(priceCreateOptions);

            //Stripe Session
            var sessionCreateOptions = new SessionCreateOptions
            {
                SuccessUrl = _webHelper.GetStoreLocation() + (!requestFromApi ? "StripeConnectRedirect/Success?session_id={CHECKOUT_SESSION_ID}" : "StripeConnectRedirect/SuccessMobileApi?session_id={CHECKOUT_SESSION_ID}"),
                CancelUrl = _webHelper.GetStoreLocation() + (!requestFromApi ? "StripeConnectRedirect/Cancel" : "StripeConnectRedirect/CancelMobileApi"),
                Mode = "payment",
                ClientReferenceId = Convert.ToString(order.OrderGuid),
                CustomerEmail = billingAddress.Email,
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = price.Id,
                        Quantity = 1
                    }
                },
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    StatementDescriptor = (!string.IsNullOrEmpty(_stripeConnectRedirectPaymentSettings.PaymentDescription) ? _stripeConnectRedirectPaymentSettings.PaymentDescription : "Alchub"),
                    TransferGroup = Convert.ToString(order.OrderGuid),
                    Metadata = new Dictionary<string, string>
                    {
                        { "OrderId", Convert.ToString(order.Id) },
                        { "OrderGuid", Convert.ToString(order.OrderGuid) }
                    },
                    CaptureMethod = "manual",
                    Description = $"{productNamePrefix}{order.Id}"
                }
            };

            var sessionService = new SessionService();
            var session = sessionService.Create(sessionCreateOptions);

            var sessionId = session.Id;
            var paymentIntentId = session.PaymentIntentId;
            var url = session.Url;

            var stripeOrder = new StripeOrder
            {
                OrderId = order.Id,
                SessionId = session.Id,
                SessionStatus = session.Status,
                PaymentIntentId = session.PaymentIntentId,
                PaymentStatus = session.PaymentStatus,
                OrderAmount = roundedOrderTotal,
                CreatedOnUtc = DateTime.UtcNow
            };

            await InsertStripeOrderAsync(stripeOrder);

            await SaveOrderTransfersAsync(order);

            return url;
        }

        #endregion Post process payment

        #region Misc

        /// <summary>
        /// Get order credit amounts (admin/vendor)
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<IDictionary<int, decimal>> GetOrderCreditAmounts(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var amountsDisc = new Dictionary<int, decimal>();
            var totalVendorCredit = decimal.Zero;

            //get orders reference records at once. 
            var stripeTransfers = (await GetStripeTransfersByOrderIdAsync(order.Id)).ToList();
            var orderItemsRefunds = (await _orderItemRefundService.GetOrderItemRefundByOrderIdAsync(orderId: order.Id)).ToList();

            if (stripeTransfers != null && stripeTransfers.Count > 0)
            {
                var vendorIds = stripeTransfers?.Select(x => x.VendorId)?.Distinct().ToArray() ?? Array.Empty<int>();
                var vendors = await _vendorService.GetVendorsByIdsAsync(vendorIds);

                foreach (var vendor in vendors)
                {
                    var vendorCredit = stripeTransfers.Where(s => s.VendorId == vendor.Id).Sum(s => s.TotalAmount)
                        - orderItemsRefunds.Where(oi => oi.VendorId == vendor.Id).Sum(oi => oi.TotalAmount)
                        + orderItemsRefunds.Where(oi => oi.VendorId == vendor.Id).Sum(oi => oi.AdminCommission);

                    //making sure key already exist exeption
                    if (amountsDisc.ContainsKey(vendor.Id))
                        amountsDisc[vendor.Id] += vendorCredit;
                    else
                        amountsDisc.Add(vendor.Id, vendorCredit > decimal.Zero ? vendorCredit : decimal.Zero);

                    totalVendorCredit += vendorCredit;
                }
            }

            //admin
            var adminCredit = order.OrderTotal - orderItemsRefunds.Sum(x => x.TotalAmount) - totalVendorCredit;
            
            //add in disctionary with vendorId = 0 (0=admin)
            amountsDisc.Add(0, adminCredit > decimal.Zero ? adminCredit : decimal.Zero);

            return amountsDisc;
        }

        #endregion

        #endregion Methods

    }
}