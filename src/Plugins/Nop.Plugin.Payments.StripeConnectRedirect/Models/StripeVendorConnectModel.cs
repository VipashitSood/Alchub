namespace Nop.Plugin.Payments.StripeConnectRedirect.Models
{
    /// <summary>
    /// Represents a Stripe Vendor Connect model
    /// </summary>
    public partial class StripeVendorConnectModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the Vendor Id
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the Is Connected
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Gets or sets the Is Refresh Required 
        /// </summary>
        public bool IsRefreshRequired { get; set; }

        /// <summary>
        /// Gets or sets the Is Verified
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Gets or sets the Information
        /// </summary>
        public string Information { get; set; }

        /// <summary>
        /// Gets or sets the error
        /// </summary>
        public string Error { get; set; }

        #endregion
    }
}