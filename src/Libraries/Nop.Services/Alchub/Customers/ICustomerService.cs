using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Customers
{
    public partial interface ICustomerService
    {
        /// <summary>
        /// Gets a value indicating whether customer is multi vendor
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<bool> IsMultiVendorAsync(Customer customer, bool onlyActiveCustomerRoles = true);

        /// <summary>
        /// Gets customers by customer identifiers
        /// </summary>
        /// <param name="customerIds">Array of customer identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the multi vendor mapped customer with vendor role
        /// </returns>
         Task<IList<Customer>> GetMultiVendorCustomerAsync(List<int> customerIds, string email = null, string name = null);
    }
}
