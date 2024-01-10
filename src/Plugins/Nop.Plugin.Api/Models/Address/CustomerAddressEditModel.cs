using Nop.Plugin.Api.DTO;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models.Address
{
    public partial record CustomerAddressEditModel 
    {
        public CustomerAddressEditModel()
        {
            Address = new AddressDto();
        }
        
        public AddressDto Address { get; set; }
    }
}