using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class LastSearchedLocation : NopViewComponent
    {
        private readonly IWorkContext _workcontext;

        public LastSearchedLocation(IWorkContext workcontext)
        {
            _workcontext = workcontext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //current customer
            var customer = await _workcontext.GetCurrentCustomerAsync();

            //return LastSearchedText as model.
            return await Task.FromResult(View("Default", customer.LastSearchedText));
        }
    }
}
