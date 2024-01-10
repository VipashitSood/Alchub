using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Tax.VendorWise.Models
{
    public record ConfigurationModel : BaseSearchModel
    {
        public ConfigurationModel()
        {
        }
        public bool Enabled { get; set; }

       
    }
}