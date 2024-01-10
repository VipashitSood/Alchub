using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Components
{
    public class AlchubCustomSearchNavigationsViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(CategoryModel categoryModel)
        {
            if (categoryModel == null)
            {
                return Content("");
            }
            return View(categoryModel);
        }
    }
}
