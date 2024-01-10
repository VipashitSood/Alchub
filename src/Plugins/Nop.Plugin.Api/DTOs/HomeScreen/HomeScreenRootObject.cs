using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.DTOs.JCarousel;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.HomeScreen
{
    public class HomeScreenRootObject
    {
        public HomeScreenRootObject()
        {
            HomePageCategories = new List<TopCategoryDto>();
            HomeBanners = new List<HomeBannerDto>();
            JCarousels = new List<JCarouselDto>();
            //FeaturedProducts = new List<FeaturedProductDto>();
            //DealsOfTheDayProducts = new List<DealsOfTheDayDto>();
        }

        [JsonProperty("homepage_categories")]
        public IList<TopCategoryDto> HomePageCategories { get; set; }

        [JsonProperty("homebanners")]
        public IList<HomeBannerDto> HomeBanners { get; set; }

        [JsonProperty("jcarousels")]
        public IList<JCarouselDto> JCarousels { get; set; }

        //[JsonProperty("featuredproducts")]
        //public IList<FeaturedProductDto> FeaturedProducts { get; set; }

        //[JsonProperty("dealsofthedayproducts")]
        //public IList<DealsOfTheDayDto> DealsOfTheDayProducts { get; set; }
    }
}
