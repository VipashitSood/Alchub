using Nop.Core.Domain.Common;
using Nop.Plugin.Api.DTO;

namespace Nop.Plugin.Api.MappingApiExtensions
{

    public static class MappingApiExtensions
    {
        public static Address ToEntity(this AddressDto model, bool trimFields = true)
        {
            if (model == null)
                return null;

            var entity = new Address();
            return ToEntity(model, entity, trimFields);
        }

        public static Address ToEntity(this AddressDto model, Address destination, bool trimFields = true)
        {
            if (model == null)
                return destination;

            if (trimFields)
            {
                if (model.FirstName != null)
                    model.FirstName = model.FirstName.Trim();
                if (model.LastName != null)
                    model.LastName = model.LastName.Trim();
                if (model.Email != null)
                    model.Email = model.Email.Trim();
              
                if (model.City != null)
                    model.City = model.City.Trim();
                if (model.Address1 != null)
                    model.Address1 = model.Address1.Trim();

                if (model.Address2 != null)
                    model.Address2 = model.Address2.Trim();

                if (model.ZipPostalCode != null)
                    model.ZipPostalCode = model.ZipPostalCode.Trim();
                if (model.PhoneNumber != null)
                    model.PhoneNumber = model.PhoneNumber.Trim();
               
            }
            destination.FirstName = model.FirstName;
            destination.LastName = model.LastName;
            destination.Email = model.Email;
            destination.CountryId = model.CountryId == 0 ? null : model.CountryId;
            destination.StateProvinceId = model.StateProvinceId == 0 ? null : model.StateProvinceId;
            destination.City = model.City;
            destination.Address1 = model.Address1;
            destination.Address2 = model.Address2;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.PhoneNumber = model.PhoneNumber;

            return destination;
        }
    }
}
