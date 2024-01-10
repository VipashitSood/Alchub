using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    public partial record CustomerModel : BaseNopEntityModel
    {
        public string VendorName { get; set; }
    }
}
