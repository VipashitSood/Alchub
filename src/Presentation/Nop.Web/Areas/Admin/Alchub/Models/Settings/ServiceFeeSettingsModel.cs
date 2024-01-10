using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Settings
{
    /// <summary>
    /// Represents a service fee setting model
    /// </summary>
    public partial record ServiceFeeSettingsModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.ServiceFee.ServiceFeeTypeId")]
        public int ServiceFeeTypeId { get; set; }
        public bool ServiceFeeTypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.ServiceFee.ServiceFee")]
        public decimal ServiceFee { get; set; }
        public bool ServiceFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.ServiceFee.ServicePercentage")]
        public decimal ServiceFeePercentage { get; set; }
        public bool ServiceFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.ServiceFee.MaximumServiceFee")]
        public decimal MaximumServiceFee { get; set; }
        public bool MaximumServiceFee_OverrideForStore { get; set; }
    }
}
