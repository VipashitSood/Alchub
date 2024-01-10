using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Caching;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Widgets.JCarousel.Domain;
using Nop.Plugin.Widgets.JCarousel.Models.Configuration;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Controllers;
using Nop.Services.Orders;
using Nop.Plugin.Widgets.JCarousel.Services;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Factories;
using Nop.Plugin.Widgets.JCarousel.Factories;

namespace Nop.Plugin.Widgets.JCarousel.PublicController
{
    public class JCarouselPublicController : BasePublicController
    {
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IJCarouselService _jCarouselService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IOrderReportService _orderReportService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IPublicJCarouselModelFactory _publicJCarouselModelFactory;
        public JCarouselPublicController(IWorkContext workContext,
            ICategoryService categoryService,
            IUrlRecordService urlRecordService,
            IStaticCacheManager staticCacheManager,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IJCarouselService jCarouselService,
            IProductService productService,
            IStoreContext storeContext,
            IOrderReportService orderReportService,
            IProductModelFactory productModelFactory,
            IPublicJCarouselModelFactory publicJCarouselModelFactory)
        {
                _workContext = workContext;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _staticCacheManager = staticCacheManager;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _jCarouselService = jCarouselService;
            _productService = productService;
            _storeContext = storeContext;
            _orderReportService = orderReportService;
            _productModelFactory = productModelFactory;
            _publicJCarouselModelFactory = publicJCarouselModelFactory;
        }

        [HttpGet]
        public async Task<IActionResult> LoadSliderData(int carouselId)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();

                var jcarousel = await _jCarouselService.GetJCarouselByIdAsync(carouselId);
                if (jcarousel == null)
                {
                    return StatusCode(404, "Slider not found");
                }
                var jcarouselModel = await _publicJCarouselModelFactory.PrepareJcarouselSliderDataModelAsync(jcarousel, customer);

                if (jcarouselModel == null)
                {
                    return StatusCode(500, "Error preparing slider data model");
                }

               
                    // Call Jcarousel Slider view component
                    return ViewComponent("JCarouselSlider", jcarouselModel);
               
            }
            catch (Exception ex)
            {
                // Handle errors and return an appropriate response, such as a 404 or 500 status code
                return StatusCode(500, "Error loading slider data: " + ex.Message);
            }
        }

    }
}
