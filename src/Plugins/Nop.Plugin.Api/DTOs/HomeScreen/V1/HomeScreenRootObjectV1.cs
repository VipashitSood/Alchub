using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.HomeScreen;

namespace Nop.Plugin.Api.DTOs.HomeScreen.V1
{
    public class HomeScreenRootObjectV1
    {
        public HomeScreenRootObjectV1()
        {
            HomePageCategories = new List<TopCategoryDto>();
            HomeBanners = new List<HomeBannerDto>();
            JCarousels = new List<JCarouselInfoDto>();
        }

        [JsonProperty("homepage_categories")]
        public IList<TopCategoryDto> HomePageCategories { get; set; }

        [JsonProperty("homebanners")]
        public IList<HomeBannerDto> HomeBanners { get; set; }

        [JsonProperty("jcarousels")]
        public IList<JCarouselInfoDto> JCarousels { get; set; }
    }
}
