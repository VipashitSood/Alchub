using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Common
{
    public partial record AddressModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Alchub.Address.Fields.GeoLocation")]
        public string GeoLocation { get; set; }

        [NopResourceDisplayName("Alchub.Address.Fields.GeoLocationCoordinates")]
        public string GeoLocationCoordinates { get; set; }

        [NopResourceDisplayName("Alchub.Address.Fields.AddressType.SaveAs")]
        public int AddressTypeId { get; set; }

        public string AddressType { get; set; }
    }
}