namespace Nop.Web.Areas.Admin.Models.Vendors
{
    /// <summary>
    /// Represents a extended vendor search model
    /// </summary>
    public partial record VendorSearchModel
    {
        #region Properties

        public bool AllowVendorCreate { get; set; }

        #endregion
    }
}