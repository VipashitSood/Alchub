using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Customers
{
    public partial class CustomerService : ICustomerService
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
        public virtual async Task<bool> IsMultiVendorAsync(Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return await IsInCustomerRoleAsync(customer, NopCustomerDefaults.MultiVendorsRole, onlyActiveCustomerRoles);
        }

        /// <summary>
        /// Gets customers by customer identifiers
        /// </summary>
        /// <param name="customerIds">Array of customer identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the multi vendor mapped customer with vendor role
        /// </returns>
        public virtual async Task<IList<Customer>> GetMultiVendorCustomerAsync(List<int> customerIds, string email = null, string name = null)
        {
            if (customerIds != null && customerIds.Contains(0))
                customerIds.Remove(0);

            if (customerIds == null)
                return new List<Customer>();

            var query = from c in _customerRepository.Table
                        where customerIds.Contains(c.Id) && !c.Deleted && c.Active
                        select c;

            var guestRole = await GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
            var customerRoleIds = new List<int> { guestRole.Id };
            if (customerRoleIds.Any())
            {
                query = query.Join(_customerCustomerRoleMappingRepository.Table, x => x.Id, y => y.CustomerId,
                        (x, y) => new { Customer = x, Mapping = y })
                    .Where(z => !customerRoleIds.Contains(z.Mapping.CustomerRoleId))
                    .Select(z => z.Customer)
                    .Distinct();
            }

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(x => x.Email.Contains(email));

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId,
                        (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                                z.Attribute.Key == NopCustomerDefaults.FirstNameAttribute &&
                                z.Attribute.Value.Contains(name))
                    .Select(z => z.Customer);
            }

            return query.ToList();
        }
    }
}
