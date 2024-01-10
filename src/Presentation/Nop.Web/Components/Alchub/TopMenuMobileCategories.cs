using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components.Alchub
{
    public class TopMenuMobileCategoriesViewComponent : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public TopMenuMobileCategoriesViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            var model = await _catalogModelFactory.PrepareTopMenuModelAsync();
            return View(model);
        }
    }
}
