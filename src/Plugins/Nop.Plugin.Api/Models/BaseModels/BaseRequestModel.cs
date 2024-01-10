using Nop.Web.Framework.Models;
using System;

namespace Nop.Plugin.Api.Models.BaseModels
{
    public partial record BaseRequestModel: BaseNopEntityModel
    {
        public int LanguageId { get; set; }
        public string PublicKey { get; set; }
        public bool IsActive { get; set; } = true;
        public int CreatedById { get; set; }
        public int UpdatedById { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }
}