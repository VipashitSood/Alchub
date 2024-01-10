using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;

namespace Nop.Services.Security
{
    /// <summary>
    /// Permission service
    /// </summary>
    public partial class PermissionService : IPermissionService
    {
        #region Methods

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the rue - authorized; otherwise, false
        /// </returns>
        public virtual async Task<bool> AuthorizeAsync(string permissionRecordSystemName, Customer customer)
        {
            if (string.IsNullOrEmpty(permissionRecordSystemName))
                return false;

            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);

            /*Alchub Start*/
            if (permissionRecordSystemName == StandardPermissionProvider.ManageDeliveryFee.SystemName)
            {
                var vendor = await _workContext.GetCurrentVendorAsync();
                if (vendor != null && !vendor.ManageDelivery)
                    return false;
            }

            if (permissionRecordSystemName == StandardPermissionProvider.ManageSlots.SystemName)
            {
                var vendor = await _workContext.GetCurrentVendorAsync();
                if (vendor != null && !vendor.ManageDelivery)
                    return false;
            }
            /*Alchub End*/

            foreach (var role in customerRoles)
                if (await AuthorizeAsync(permissionRecordSystemName, role.Id))
                    //yes, we have such permission
                    return true;

            //no permission found
            return false;
        }

        #endregion
    }
}