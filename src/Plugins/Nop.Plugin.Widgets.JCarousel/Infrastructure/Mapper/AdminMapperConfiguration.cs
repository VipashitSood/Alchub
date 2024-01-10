using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Widgets.JCarousel.Domain;
using Nop.Plugin.Widgets.JCarousel.Models.Configuration;
namespace Nop.Web.Areas.Admin.Infrastructure.Mapper
{
    /// <summary>
    /// AutoMapper configuration for admin area models
    /// </summary>
    public class AdminMapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public AdminMapperConfiguration()
        {
            //create specific maps
            CreateJcarouselMaps();
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Create jcarouel maps 
        /// </summary>
        protected virtual void CreateJcarouselMaps()
        {
            CreateMap<JCarouselLog, JCarouselModel>()
                .ForMember(model => model.DotNevigation, options => options.Ignore())
                .ForMember(model => model.ArrowNevigation, options => options.Ignore());

            CreateMap<JCarouselModel, JCarouselLog>()
                .ForMember(entity => entity.DotNevigation, options => options.Ignore())
                .ForMember(entity => entity.ArrowNevigation, options => options.Ignore());

            CreateMap<ProductJCarouselMapping, ProductJCarouselMappingModel>()
                 .ForMember(model => model.ProductName, options => options.Ignore());

            CreateMap<ProductJCarouselMappingModel, ProductJCarouselMapping>()
                .ForMember(entity => entity.JCarouselId, options => options.Ignore())
                .ForMember(entity => entity.ProductId, options => options.Ignore());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => 0;

        #endregion
    }
}