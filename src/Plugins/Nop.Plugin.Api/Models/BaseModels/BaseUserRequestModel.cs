using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.BaseModels
{
    public partial record BaseUserRequestModel : BaseRequestModel
    {
        public int UserId { get; set; }

        public int BrandId { get; set; }

        public int AreaId { get; set; }

        public int RetailerId { get; set; }

    }
}
