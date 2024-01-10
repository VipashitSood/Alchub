using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Services.Alchub.General;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.ScheduleTasks;
using StackExchange.Redis;

namespace Nop.Services.Alchub.Catalog
{
    public partial class SyncProductsImagesTask : IScheduleTask
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        #endregion

        #region Ctor
        public SyncProductsImagesTask(
            ILogger logger,
            IProductService productService,
            IPictureService pictureService,
            IWebHelper webHelper)
        {

            _logger = logger;
            _productService = productService;
            _pictureService = pictureService;
            _webHelper = webHelper;
        }
        #endregion
        private async Task SyncProductImages()
        {

            var products = await _productService.GetAllProductsAsync();
            if (products.Any())
            {
                foreach (Product product in products)
                {
                    try
                    {
                      
                        string sku = product.Sku;
                        //IF product is group product, then get SKU from associated from products to update image from CDN
                        if (string.IsNullOrEmpty(product.Sku) && product.ProductType==ProductType.GroupedProduct)
                        {
                            // Go get Associated products
                            var associatedProducts = products.Where(x => x.ParentGroupedProductId == product.Id && x.ProductTypeId==(int)ProductType.SimpleProduct && x.Sku is not null).ToList();
                            if (associatedProducts.Any())
                            {
                                Product firstGroupedProduct = associatedProducts.FirstOrDefault();
                                if (firstGroupedProduct != null)
                                {
                                    // assign SKU code from associated products from image retrival
                                    sku = firstGroupedProduct.Sku;
                                }
                            }
                        }
                        string imageUrl = await _pictureService.GetProductPictureUrlAsync(sku, 0, null, PictureType.Entity);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            // Update image to associated product 
                            product.ImageUrl = imageUrl;
                            product.UpdatedOnUtc = DateTime.UtcNow;
                            await _productService.UpdateProductAsync(product);
                        }
                    }
                    catch (Exception ex) //Todo: for log if any error will throw
                    {
                        await _logger.InsertLogAsync(LogLevel.Error, "ProductId:" + product.Id + ", ProductName:" + product.Name + ", Exception:" + ex.Message, ex.StackTrace);
                    }
                }
            }
        }

        #region Methods

        public virtual async Task ExecuteAsync()
        {
            try
            {
                await SyncProductImages();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(string.Format("SyncProductsImagesTask Error occured: during sync same name of products getting error : {0}. ", ex.Message), ex);
            }

        }

        #endregion
    }
}
