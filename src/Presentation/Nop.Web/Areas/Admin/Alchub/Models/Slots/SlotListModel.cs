using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Slots
{
    /// <summary>
    /// Represents a slot list model
    /// </summary>
    public partial record SlotListModel : BasePagedListModel<SlotModel>
    {
    }
}