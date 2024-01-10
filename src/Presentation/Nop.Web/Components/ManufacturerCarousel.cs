using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class ManufacturerCarousel : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly CatalogSettings _catalogSettings;

        public ManufacturerCarousel(ICatalogModelFactory catalogModelFactory,
            CatalogSettings catalogSettings)
        {
            _catalogModelFactory = catalogModelFactory;
            _catalogSettings = catalogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //++Alchub

            if (_catalogSettings.NumberOfManufacturersOnHomepage <= 0)
                return Content("");

            //var model = await _catalogModelFactory.PrepareManufacturerAllModelsAsync();
            var model = await _catalogModelFactory.PrepareManufacturersModelsAsync(pageIndex: 0, pageSize: _catalogSettings.NumberOfManufacturersOnHomepage);

            //-- Alchub
            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}
