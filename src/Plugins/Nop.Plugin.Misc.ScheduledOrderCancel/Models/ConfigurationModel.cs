using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ScheduledOrderCancel.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ScheduledOrderCancel.Fields.Enabled")]
        public bool Enabled { get; set; }
        public bool Enabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ScheduledOrderCancel.Fields.Time")]
        public int Time { get; set; }
        public bool Time_OverrideForStore { get; set; }
    }
}