using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.JCarousel.Models.Configuration
{
    public record JCarouselSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Widgets.List.SearchJCarouselName")]
        public string SearchJCarouselName { get; set; }
    }
}
