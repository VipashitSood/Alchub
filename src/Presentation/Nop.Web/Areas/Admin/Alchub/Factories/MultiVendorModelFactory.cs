using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.MultiVendors;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Models.Extensions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;

namespace Nop.Web.Areas.Admin.Alchub.Factories
{
    public partial class MultiVendorModelFactory : IMultiVendorModelFactory
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorService _vendorService;
        private readonly IMultiVendorService _multiVendorService;
        private readonly IWorkContext _workContext;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;

        #endregion

        #region Ctor

        public MultiVendorModelFactory(ICustomerService customerService,
            ILocalizationService localizationService,
            IVendorService vendorService,
            IMultiVendorService multiVendorService,
            IWorkContext workContext,
            IAclSupportedModelFactory aclSupportedModelFactory)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _vendorService = vendorService;
            _multiVendorService = multiVendorService;
            _workContext = workContext;
            _aclSupportedModelFactory = aclSupportedModelFactory;
        }

        #endregion

        /// <summary>
        /// Prepare multi vendor search model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the multi vendor search model
        /// </returns>
        public virtual async Task<CustomerSearchModel> PrepareMultiVendorSearchModelAsync(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //search registered customers by default
            var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
            if (registeredRole != null)
                searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

            //prepare available customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(searchModel);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged customer list model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer list model
        /// </returns>
        public virtual async Task<CustomerListModel> PrepareMultiVendorListModelAsync(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter customers
            _ = int.TryParse(searchModel.SearchDayOfBirth, out var dayOfBirth);
            _ = int.TryParse(searchModel.SearchMonthOfBirth, out var monthOfBirth);

            //get customers
            var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: searchModel.SelectedCustomerRoleIds.ToArray(),
                email: searchModel.SearchEmail,
                username: searchModel.SearchUsername,
                firstName: searchModel.SearchFirstName,
                lastName: searchModel.SearchLastName,
                dayOfBirth: dayOfBirth,
                monthOfBirth: monthOfBirth,
                company: searchModel.SearchCompany,
                phone: searchModel.SearchPhone,
                zipPostalCode: searchModel.SearchZipPostalCode,
                ipAddress: searchModel.SearchIpAddress,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new CustomerListModel().PrepareToGridAsync(searchModel, customers, () =>
            {
                return customers.SelectAwait(async customer =>
                {
                    //fill in model values from the entity
                    var customerModel = customer.ToModel<CustomerModel>();

                    //convert dates to the user time
                    customerModel.Email = (await _customerService.IsRegisteredAsync(customer))
                        ? customer.Email
                        : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
                    customerModel.FullName = await _customerService.GetCustomerFullNameAsync(customer);

                    return customerModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare paged multi vendor assosiated vendor list model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the multi vendor assosiated vendor list model
        /// </returns>
        public virtual async Task<CustomerListModel> PrepareMultiVendorAssosiatedVendorListModelAsync(CustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var currentMultiVendor = await _workContext.GetCurrentMultiVendorAsync();
            if (currentMultiVendor == null)
                return new CustomerListModel();

            var customerWithVendorRoleIds = await _multiVendorService.GetVendorIdsByMultiVendorAsync(currentMultiVendor.Id);

            //get customers
            var customersWithVendorRole = (await _customerService.GetMultiVendorCustomerAsync(customerWithVendorRoleIds, searchModel.SearchEmail,
                searchModel.SearchFirstName))?.ToPagedList(searchModel);

            //prepare list model
            var model = await new CustomerListModel().PrepareToGridAsync(searchModel, customersWithVendorRole, () =>
            {
                return customersWithVendorRole.SelectAwait(async customer =>
                {
                    //fill in model values from the entity
                    var customerModel = customer.ToModel<CustomerModel>();
                    customerModel.Email = customer.Email;
                    customerModel.FullName = await _customerService.GetCustomerFullNameAsync(customer);

                    var vendor = await _vendorService.GetVendorByIdAsync(customer.VendorId);
                    if (!string.IsNullOrEmpty(vendor?.Name))
                        customerModel.VendorName = vendor.Name;

                    return customerModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare Multi vendor model
        /// </summary>
        /// <param name="multiVendorModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public virtual async Task<MultiVendorModel> PrepareMultiVendorModel(MultiVendorModel multiVendorModel, Customer customer)
        {
            if (customer != null)
            {
                if (multiVendorModel == null)
                {
                    multiVendorModel = new MultiVendorModel();
                    var currentVendorIds = await _multiVendorService.GetVendorIdsByMultiVendorAsync(customer.Id);
                    multiVendorModel.Name = customer.Email;
                    multiVendorModel.Id = customer.Id;
                    multiVendorModel.SelectedVendorIds = currentVendorIds;
                }
            }
            var vendorRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.VendorsRoleName);
            var roleIds = new List<int> { vendorRole.Id };
            var customersWithVendorRole = await _customerService.GetAllCustomersAsync(customerRoleIds: roleIds.ToArray());

            //prepare available vendors
            foreach (var item in customersWithVendorRole)
            {
                var vendor = await _vendorService.GetVendorByIdAsync(item.VendorId);
                if (vendor == null)
                    continue;

                var name = await _customerService.GetCustomerFullNameAsync(item);
                multiVendorModel.AvailableVendors.Add(new SelectListItem
                {
                    Text = !string.IsNullOrEmpty(vendor.Name) ? name + "(" + vendor?.Name + ")" : name,
                    Value = item.Id.ToString(),
                    Selected = multiVendorModel.SelectedVendorIds.Contains(item.Id)
                });
            }


            return multiVendorModel;
        }

    }
}
