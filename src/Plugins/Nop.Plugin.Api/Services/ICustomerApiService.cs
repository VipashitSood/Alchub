using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Api.DTO.Customers;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public interface ICustomerApiService
    {
        Task<int> GetCustomersCountAsync();

        Task<CustomerDto> GetCustomerByIdAsync(int id, bool showDeleted = false);

        Task<Customer> GetCustomerEntityByIdAsync(int id);

        Task<IList<CustomerDto>> GetCustomersDtosAsync(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            int limit = Constants.Configurations.DEFAULT_LIMIT, int page = Constants.Configurations.DEFAULT_PAGE_VALUE,
            int sinceId = Constants.Configurations.DEFAULT_SINCE_ID);

        Task<IList<CustomerDto>> SearchAsync(
            string query = "", string order = Constants.Configurations.DEFAULT_ORDER,
            int page = Constants.Configurations.DEFAULT_PAGE_VALUE, int limit = Constants.Configurations.DEFAULT_LIMIT);

        Task<Dictionary<string, string>> GetFirstAndLastNameByCustomerIdAsync(int customerId);

        /// <summary>
        /// Gets current user working language
        /// </summary>
        Task<Language> GetCustomerLanguageAsync(Customer customer);

        /// <summary>
        /// Sets current user working language
        /// </summary>
        /// <param name="language">Language</param>
        Task SetCustomerLanguageAsync(Customer customer, Language language);

        /// <summary>
        /// Gets or sets current user working currency
        /// </summary>
        Task<Currency> GetCustomerCurrencyAsync(Customer customer);

        /// <summary>
        /// Sets current user working currency
        /// </summary>
        /// <param name="currency">Currency</param>
        Task SetCustomerCurrencyAsync(Customer customer, Currency currency);

        /// <summary>
        /// Gets a list of addresses mapped to customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 
        /// </returns>
        Task<IPagedList<Address>> GetAddressesByCustomerIdAsync(int customerId,
              int? pageIndex = 0,
          int? pageSize = int.MaxValue,
          int storeId = 0);
    }
}
