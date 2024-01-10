using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Alchub.Domain;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class LocationSearchBox : NopViewComponent
    {
        private readonly AlchubSettings _alchubSettings;

        public LocationSearchBox(AlchubSettings alchubSettings)
        {
            _alchubSettings = alchubSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //return google key in model.
            return await Task.FromResult(View("Default", _alchubSettings.GoogleApiKey));
        }
    }
}
