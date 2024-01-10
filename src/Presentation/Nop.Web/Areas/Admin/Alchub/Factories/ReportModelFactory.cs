using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Reports;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the report model factory implementation
    /// </summary>
    public partial class ReportModelFactory : IReportModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerReportService _customerReportService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderReportService _orderReportService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ReportModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICustomerReportService customerReportService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IOrderReportService orderReportService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IProductService productService,
            IWorkContext workContext)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _customerReportService = customerReportService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _orderReportService = orderReportService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatter = productAttributeFormatter;
            _productService = productService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task<IPagedList<BestsellersReportLine>> GetBestsellersReportAsync(BestsellerSearchModel searchModel)
        {
            //get parameters to filter bestsellers
            var orderStatus = searchModel.OrderStatusId > 0 ? (OrderStatus?)searchModel.OrderStatusId : null;
            var paymentStatus = searchModel.PaymentStatusId > 0 ? (PaymentStatus?)searchModel.PaymentStatusId : null;
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
                searchModel.VendorId = currentVendor.Id;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //prepare isMaster value
            var isMasterProductsOnly = false;
            //Note: If admin then he can see master product report.
            if (await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                isMasterProductsOnly = true;

            //get bestsellers
            var bestsellers = await _orderReportService.BestSellersReportAsync(showHidden: true,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                billingCountryId: searchModel.BillingCountryId,
                orderBy: OrderByEnum.OrderByTotalAmount,
                vendorId: searchModel.VendorId,
                categoryId: searchModel.CategoryId,
                manufacturerId: searchModel.ManufacturerId,
                storeId: searchModel.StoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                isMasterOnly: isMasterProductsOnly);

            return bestsellers;
        }

        #endregion
    }
}