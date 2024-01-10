using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.JCarousel.Models.Configuration
{
    public record JCarouselModel : BaseNopEntityModel
    {
        public JCarouselModel()
        {
            AvailableDataSourceType = new List<SelectListItem>();
            JCarouselProductSearchModel = new JCarouselProductSearchModel();
            JcarouselProductsModel = new JcarouselProductsModel();
            AvailableCategories = new List<SelectListItem>();
        }
        
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.DataSourceName")]
        public string DataSourceName { get; set; }
        //[NopResourceDisplayName("Plugins.Widgets.JCarousel.MaxItems")]
        //public int MaxItems { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.VisibleItems")]
        public int VisibleItems { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Skin")]
        public string Skin { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.StartIndex")]
        public int StartIndex { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.ScrollItems")]
        public int ScrollItems { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.DotNevigation")]
        public bool DotNevigation { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.ArrowNevigation")]
        public bool ArrowNevigation { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.DataSourceTypeId")]
        public int DataSourceTypeId { get; set; }
        public IList<SelectListItem> AvailableDataSourceType { get; set; }
        public JCarouselProductSearchModel JCarouselProductSearchModel { get; set; }
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.DisplayOrder")]
        public int DisplayOrder { get; set; }
        public JcarouselProductsModel JcarouselProductsModel { get; set; }
        //category
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.SelectedCategoryId")]
        public int SelectedCategoryId { get; set; }
        public string SeName { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
        //category id saved in db
        public int CategoryId { get; set; }
        //To prepare view all url
        public int ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.IsBestSeller")]
        public bool IsBestSeller { get; set; }
    }
}
