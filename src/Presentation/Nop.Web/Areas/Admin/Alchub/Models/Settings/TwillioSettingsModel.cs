using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Alchub.Models.Settings
{
    /// <summary>
    /// Represents a vendor settings model
    /// </summary>
    public partial record TwillioSettingsModel : BaseNopModel, ISettingsModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.Enabled")]
        public bool Enabled { get; set; }
        public bool Enabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.AccountSid")]
        public string AccountSid { get; set; }
        public bool AccountSid_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.AuthToken")]
        public string AuthToken { get; set; }
        public bool AuthToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.FromNumber")]
        public string FromNumber { get; set; }
        public bool FromNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.DefaultCountryCode")]
        public string DefaultCountryCode { get; set; }
        public bool DefaultCountryCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.OrderPlacedBody")]
        public string OrderPlacedBody { get; set; }
        public bool OrderPlacedBody_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.OrderItemsDispatchedBody")]
        public string OrderItemsDispatchedBody { get; set; }
        public bool OrderItemsDispatchedBody_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.OrderItemsPickedUpBody")]
        public string OrderItemsPickedUpBody { get; set; }
        public bool OrderItemsPickedUpBody_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.OrderItemsDeliveredBody")]
        public string OrderItemsDeliveredBody { get; set; }
        public bool OrderItemsDeliveredBody_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.OrderItemsCancelBody")]
        public string OrderItemsCancelBody { get; set; }
        public bool OrderItemsCancelBody_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.OrderItemsDelivereyDeniedBody")]
        public string OrderItemsDelivereyDeniedBody { get; set; }
        public bool OrderItemsDelivereyDeniedBody_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.OrderItemPickupBody")]
        public string OrderItemPickupBody { get; set; }
        public bool OrderItemPickupBody_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Twillio.OrderPlacedVendorBody")]
        public string OrderPlacedVendorBody { get; set; }
        public bool OrderPlacedVendorBody_OverrideForStore { get; set; }

        #endregion
    }
}
