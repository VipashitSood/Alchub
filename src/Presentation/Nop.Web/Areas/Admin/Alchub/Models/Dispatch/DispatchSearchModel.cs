using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Dispatch
{
    /// <summary>
    /// Represents a blog post search model
    /// </summary>
    public partial record DispatchSearchModel : BaseSearchModel
    {

        #region Properties

        public int VendorId { get; set; }
 
        #endregion
    }
}