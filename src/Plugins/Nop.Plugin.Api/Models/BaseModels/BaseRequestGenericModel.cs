using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models.BaseModels
{
    public record BaseRequestGenericModel : BaseNopEntityModel
    {
        /// <summary>
        /// Gets or sets languageId
        /// </summary>
        public int LanguageId { get; set; } = 1;
    }
}
