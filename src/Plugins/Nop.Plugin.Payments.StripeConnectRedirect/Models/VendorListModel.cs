using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Models
{
    /// <summary>
    /// Represents a stripe connect vendor list model
    /// </summary>
    public partial record VendorListModel : BasePagedListModel<StripeVendorModel>
    {

    }
}