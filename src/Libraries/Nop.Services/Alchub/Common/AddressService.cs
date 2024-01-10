using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Services.Directory;

namespace Nop.Services.Common
{
    /// <summary>
    /// Address service
    /// </summary>
    public partial class AddressService : IAddressService
    {
        #region Methods

        /// <summary>
        /// Clone address
        /// </summary>
        /// <returns>A deep copy of address</returns>
        public virtual Address CloneAddress(Address address)
        {
            var addr = new Address
            {
                FirstName = address.FirstName,
                LastName = address.LastName,
                Email = address.Email,
                Company = address.Company,
                CountryId = address.CountryId,
                StateProvinceId = address.StateProvinceId,
                County = address.County,
                City = address.City,
                Address1 = address.Address1,
                Address2 = address.Address2,
                ZipPostalCode = address.ZipPostalCode,
                PhoneNumber = address.PhoneNumber,
                FaxNumber = address.FaxNumber,
                CustomAttributes = address.CustomAttributes,
                CreatedOnUtc = address.CreatedOnUtc,
                //++Alchub
                GeoLocation = address.GeoLocation,
                GeoLocationCoordinates = address.GeoLocationCoordinates,
                AddressTypeId = address.AddressTypeId,
                //--Alchub
            };

            return addr;
        }

        #endregion
    }
}