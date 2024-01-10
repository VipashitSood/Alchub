using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.JCarousel.Models.Configuration
{
     public partial record AddProductToJCarouselSearchModel : BaseSearchModel
     {
        #region Ctor

        public AddProductToJCarouselSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Products.List.SearchProductName")]
        public string SearchProductName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Products.List.SearchCategory")]
        public int SearchCategoryId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Products.List.SearchManufacturer")]
        public int SearchManufacturerId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Products.List.SearchStore")]
        public int SearchStoreId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Products.List.SearchVendor")]
        public int SearchVendorId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Products.List.SearchProductType")]
        public int SearchProductTypeId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        public IList<SelectListItem> AvailableProductTypes { get; set; }


        #endregion
    }
}
