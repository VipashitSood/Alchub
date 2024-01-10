using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Forefront.Xero.Domain;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Model;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace Nop.Plugin.Forefront.Xero.Services
{
	public class XeroProductService : IXeroProductService
	{
        #region Fields

        private readonly IRepository<XeroProduct> _xeroProductRepository;
		private readonly IProductService _productService;
		private readonly ISettingService _settingService;
        private readonly IXeroAccessRefreshTokenService _xeroAccessRefreshTokenService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public XeroProductService(IRepository<XeroProduct> xeroProductRepository, IProductService productService, ISettingService settingService, 
            IXeroAccessRefreshTokenService xeroAccessRefreshTokenService, IStoreContext storeContext)
		{
			_xeroProductRepository = xeroProductRepository;
			_productService = productService;
			_settingService = settingService;
            _xeroAccessRefreshTokenService = xeroAccessRefreshTokenService;
            _storeContext = storeContext;
		}

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="isDeleted"></param>
        /// <param name="actionType"></param>
        /// <param name="xeroStatus"></param>
        public async Task ChangeXeroProductStatus(int productId, bool isDeleted, string actionType, bool xeroStatus = false)
		{
			var xeroProductByProductId = await GetXeroProductByProductId(productId);
			if (xeroProductByProductId == null)
			{
				await InsertXeroProductByProductId(productId, "Create");
			}
			else
			{
				xeroProductByProductId.IsDeleted = isDeleted;
				xeroProductByProductId.XeroStatus = xeroStatus;
				xeroProductByProductId.InTime = DateTime.UtcNow;
				xeroProductByProductId.ActionType = actionType;
				xeroProductByProductId.SyncAttemptCount = 0;
				await UpdateXeroProduct(xeroProductByProductId);
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="products"></param>
		public async Task CreateXeroItem(IList<Product> products)
		{
            var record = await _xeroAccessRefreshTokenService.CheckRecordExist();
            if (record != null)
            {
                IAccountingApi accountingApi = new AccountingApi();

                string accountCode;
                if (products.Count > 0)
                {
                    int activeStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                    var setting = await  _settingService.LoadSettingAsync<ForeFrontXeroSetting>(activeStoreScopeConfiguration);

                    int num = 0;
                    num = setting.BatchSize <= 0 ? 100 : setting.BatchSize;

                    decimal number = new decimal();
                    number = products.Count > num ? products.Count / num : new decimal();

                    accountCode = string.IsNullOrEmpty(setting.SalesAccountCode) ? "200" : setting.SalesAccountCode;
                    for (int i = 0; i <= Math.Round(number); i++)
                    {
                        var itemsApi =  accountingApi.GetItemsAsync(record.AccessToken, record.Tenants[0].TenantId.ToString()).GetAwaiter();
                        IList<Item> itemList = itemsApi.GetResult()._Items;

                        try
                        {
                            var productsForInventoryByFixedBatch =await GetProductsForInventoryByFixedBatch();
                            if (productsForInventoryByFixedBatch.Count > 0)
                            {
                                var productsByIds = await _productService.GetProductsByIdsAsync(productsForInventoryByFixedBatch.Select(p => p.ProductId).ToArray());
                                productsByIds = productsByIds.Where(p => !string.IsNullOrEmpty(p.UPCCode)).Take(num).ToList();

                                var list = new List<Item>();
                                if (productsByIds.Count > 0)
                                {
                                    foreach (var product in productsByIds)
                                    {
                                        var item = itemList.FirstOrDefault(p => p.Code == product.UPCCode);
                                        var itemDetail = new Item()
                                        {
                                            Code = product.UPCCode,
                                            Name = product.Name,
                                            IsPurchased = true,
                                            IsSold = true
                                        };

                                        if (string.IsNullOrEmpty(product.ShortDescription))
                                        {
                                            itemDetail.Description = product.Name;
                                        }
                                        else
                                        {
                                            itemDetail.Description = Regex.Replace(product.ShortDescription, "<.*?>", string.Empty);
                                        }

                                        //itemDetail.UpdatedDateUTC = DateTime.UtcNow;
                                        itemDetail.SalesDetails = new Purchase();
                                        itemDetail.PurchaseDetails = new Purchase();
                                        itemDetail.SalesDetails.UnitPrice = product.Price;
                                        itemDetail.SalesDetails.AccountCode = accountCode;

                                        if (item != null)
                                        {
                                            itemDetail.IsTrackedAsInventory = item.IsTrackedAsInventory;
                                            if (item.IsTrackedAsInventory.HasValue && !item.IsTrackedAsInventory.Value)
                                            {
                                                itemDetail.PurchaseDetails.UnitPrice = product.ProductCost;

                                                if (!string.IsNullOrEmpty(item.PurchaseDetails.AccountCode))
                                                {
                                                    itemDetail.PurchaseDetails.AccountCode = item.PurchaseDetails.AccountCode;
                                                }
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(item.InventoryAssetAccountCode))
                                                {
                                                    itemDetail.InventoryAssetAccountCode = item.InventoryAssetAccountCode;
                                                }

                                                if (!string.IsNullOrEmpty(item.PurchaseDetails.COGSAccountCode))
                                                {
                                                    itemDetail.PurchaseDetails.COGSAccountCode = item.PurchaseDetails.COGSAccountCode;
                                                }

                                                itemDetail.PurchaseDetails.UnitPrice = product.ProductCost;
                                            }
                                        }
                                        else if (!string.IsNullOrEmpty(setting.PurchaseAccountCode))
                                        {
                                            itemDetail.PurchaseDetails.UnitPrice = product.ProductCost;
                                            itemDetail.PurchaseDetails.AccountCode = setting.PurchaseAccountCode;
                                        }
                                        else
                                        {

                                        }

                                        list.Add(itemDetail);
                                    }
                                }

                                try
                                {
                                    if (list.Count > 0)
                                    {
                                        var items = new Items();
                                        items._Items = new List<Item>();
                                        items._Items.AddRange(list);
                                        itemsApi = accountingApi.UpdateOrCreateItemsAsync(record.AccessToken, record.Tenants[0].TenantId.ToString(), items, false).GetAwaiter();
                                        itemList = itemsApi.GetResult()._Items;

                                        foreach (var result in itemList)
                                        {
                                            if (result.ValidationErrors.Count > 0)
                                            {
                                                var productSku = await _productService.GetProductBySkuAsync(result.Code);
                                                int prodId = (productSku != null ? productSku.Id : 0);
                                                if (prodId > 0)
                                                {
                                                    var xeroProduct = await GetXeroProductByProductId(prodId);
                                                    xeroProduct.SyncAttemptCount = xeroProduct.SyncAttemptCount + 1;
                                                    await UpdateXeroProduct(xeroProduct);
                                                }
                                            }
                                            else
                                            {
                                                var productSku = await _productService.GetProductBySkuAsync(result.Code);
                                                int productId = (productSku != null ? productSku.Id : 0);
                                                if (productId > 0)
                                                {
                                                    var xeroProductByProductId = await GetXeroProductByProductId(productId);
                                                    xeroProductByProductId.XeroStatus = true;
                                                    xeroProductByProductId.XeroId = result.ItemID.ToString();
                                                    xeroProductByProductId.SyncAttemptCount = xeroProductByProductId.SyncAttemptCount + 1;
                                                   await UpdateXeroProduct(xeroProductByProductId);
                                                    if (itemList.FirstOrDefault(p => p.Code == result.Code) == null)
                                                    {
                                                        itemList.Add(result);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception )
                                {

                                }
                            }
                        }
                        catch (Exception )
                        {

                        }
                    }
                }
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="products"></param>
        public async Task CreateXeroItemOrderTime(IList<Product> products)
        {
            var record = await _xeroAccessRefreshTokenService.CheckRecordExist();
            if (record != null)
            {
                IAccountingApi accountingApi = new AccountingApi();

                string accountCode;
                if (products.Count > 0)
                {
                    int activeStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                    var setting = await _settingService.LoadSettingAsync<ForeFrontXeroSetting>(activeStoreScopeConfiguration);

                    int num = 0;
                    num = setting.BatchSize <= 0 ? 100 : setting.BatchSize;

                    decimal number = new decimal();
                    number = products.Count > num ? products.Count / num : new decimal();

                    accountCode = string.IsNullOrEmpty(setting.SalesAccountCode) ? "200" : setting.SalesAccountCode;
                    for (int i = 0; i <= Math.Round(number); i++)
                    {
                        var itemsApi = accountingApi.GetItemsAsync(record.AccessToken, record.Tenants[0].TenantId.ToString()).GetAwaiter();
                        IList<Item> itemList = itemsApi.GetResult()._Items;
                        try
                        { 
                                var list = new List<Item>();
                                if (products.Count > 0)
                                {
                                    foreach (var product in products)
                                    {
                                        var item = itemList.FirstOrDefault(p => p.Code == product.UPCCode);
                                        var itemDetail = new Item()
                                        {
                                            Code = product.UPCCode,
                                            Name = product.Name,
                                            IsPurchased = true,
                                            IsSold = true
                                        };

                                        if (string.IsNullOrEmpty(product.ShortDescription))
                                        {
                                            itemDetail.Description = product.Name;
                                        }
                                        else
                                        {
                                            itemDetail.Description = Regex.Replace(product.ShortDescription, "<.*?>", string.Empty);
                                        }

                                        //itemDetail.UpdatedDateUTC = DateTime.UtcNow;
                                        itemDetail.SalesDetails = new Purchase();
                                        itemDetail.PurchaseDetails = new Purchase();
                                        itemDetail.SalesDetails.UnitPrice = product.Price;
                                        itemDetail.SalesDetails.AccountCode = accountCode;

                                        if (item != null)
                                        {
                                            itemDetail.IsTrackedAsInventory = item.IsTrackedAsInventory;
                                            if (item.IsTrackedAsInventory.HasValue && !item.IsTrackedAsInventory.Value)
                                            {
                                                itemDetail.PurchaseDetails.UnitPrice = product.ProductCost;

                                                if (!string.IsNullOrEmpty(item.PurchaseDetails.AccountCode))
                                                {
                                                    itemDetail.PurchaseDetails.AccountCode = item.PurchaseDetails.AccountCode;
                                                }
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(item.InventoryAssetAccountCode))
                                                {
                                                    itemDetail.InventoryAssetAccountCode = item.InventoryAssetAccountCode;
                                                }

                                                if (!string.IsNullOrEmpty(item.PurchaseDetails.COGSAccountCode))
                                                {
                                                    itemDetail.PurchaseDetails.COGSAccountCode = item.PurchaseDetails.COGSAccountCode;
                                                }

                                                itemDetail.PurchaseDetails.UnitPrice = product.ProductCost;
                                            }
                                        }
                                        else if (!string.IsNullOrEmpty(setting.PurchaseAccountCode))
                                        {
                                            itemDetail.PurchaseDetails.UnitPrice = product.ProductCost;
                                            itemDetail.PurchaseDetails.AccountCode = setting.PurchaseAccountCode;
                                        }
                                        else
                                        {

                                        }

                                        list.Add(itemDetail);
                                    }
                                }

                                try
                                {
                                    if (list.Count > 0)
                                    {
                                        var items = new Items();
                                        items._Items = new List<Item>();
                                        items._Items.AddRange(list);
                                        itemsApi = accountingApi.UpdateOrCreateItemsAsync(record.AccessToken, record.Tenants[0].TenantId.ToString(), items, false).GetAwaiter();
                                        itemList = itemsApi.GetResult()._Items;

                                        foreach (var result in itemList)
                                        {
                                            if (result.ValidationErrors.Count > 0)
                                            {
                                                var productSku = await _productService.GetProductBySkuAsync(result.Code);
                                                int prodId = (productSku != null ? productSku.Id : 0);
                                                if (prodId > 0)
                                                {
                                                    var xeroProduct = await GetXeroProductByProductId(prodId);
                                                    xeroProduct.SyncAttemptCount = xeroProduct.SyncAttemptCount + 1;
                                                    await UpdateXeroProduct(xeroProduct);
                                                }
                                            }
                                            else
                                            {
                                                var productSku = await _productService.GetProductBySkuAsync(result.Code);
                                                int productId = (productSku != null ? productSku.Id : 0);
                                                if (productId > 0)
                                                {
                                                    var xeroProductByProductId = await GetXeroProductByProductId(productId);
                                                    xeroProductByProductId.XeroStatus = true;
                                                    xeroProductByProductId.XeroId = result.ItemID.ToString();
                                                    xeroProductByProductId.SyncAttemptCount = xeroProductByProductId.SyncAttemptCount + 1;
                                                    await UpdateXeroProduct(xeroProductByProductId);
                                                    if (itemList.FirstOrDefault(p => p.Code == result.Code) == null)
                                                    {
                                                        itemList.Add(result);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {

                                }
                            
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
		public virtual async Task<IPagedList<XeroProduct>> GetAllXeroProduct(int pageIndex, int pageSize)
		{
            var query = _xeroProductRepository.Table;
            query = query.OrderByDescending(o => o.XeroId);
            
			return new PagedList<XeroProduct>(query.ToList(), pageIndex, pageSize);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IList<XeroProduct>> GetProductsForInventory()
        {
            var xeroProductList = (from q in _xeroProductRepository.Table
                where !q.XeroStatus && !q.IsDeleted && q.SyncAttemptCount < 3
                select q).ToList();

            return xeroProductList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IList<XeroProduct>> GetProductsForInventoryByFixedBatch()
        {
            var xeroProductList = (from q in _xeroProductRepository.Table
                where !q.XeroStatus && !q.IsDeleted && q.SyncAttemptCount < 3
                select q).Take(500).ToList();

            return xeroProductList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task <XeroProduct> GetXeroProductByProductId(int productId)
        {
            var xeroProduct = (from i in _xeroProductRepository.Table
                where i.ProductId == productId
                select i).FirstOrDefault();

            return xeroProduct;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroProduct"></param>
        public async Task InsertXeroProduct(XeroProduct xeroProduct)
        {
            if (xeroProduct == null)
            {
                throw new ArgumentNullException("XeroProduct");
            }

           await _xeroProductRepository.InsertAsync(xeroProduct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="actionType"></param>
        public async Task InsertXeroProductByProductId(int productId, string actionType)
        {
            if (await GetXeroProductByProductId(productId) == null)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                var xeroProduct = new XeroProduct()
                {
                    ProductId = productId,
                    IsDeleted = product.Deleted,
                    XeroStatus = true,
                    InTime = DateTime.UtcNow,
                    ActionType = actionType,
                    SyncAttemptCount = 0
                };

               await InsertXeroProduct(xeroProduct);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task ManageProductQuantity()
        {
            try
            {
                var record =  await _xeroAccessRefreshTokenService.CheckRecordExist();
                if (record != null)
                {
                    IAccountingApi accountingApi = new AccountingApi();
                    DateTime utcNow = DateTime.UtcNow;
                    var itemsApi = accountingApi.GetItemsAsync(record.AccessToken, record.Tenants[0].TenantId.ToString(), utcNow.AddDays(-1)).GetAwaiter();
                    IList<Item> items = itemsApi.GetResult()._Items;
                    items = items.Where(item => item.IsTrackedAsInventory.HasValue && item.IsTrackedAsInventory.Value == true).ToList();

                    foreach (var item in items)
                    {
                        if (item.QuantityOnHand.HasValue)
                        {
                            var productBySku =await _productService.GetProductBySkuAsync(item.Code);
                            if (productBySku != null)
                            {
                                decimal? quantityOnHand = Convert.ToDecimal(item.QuantityOnHand.Value);
                                int stockQuantity = productBySku.StockQuantity;
                                if ((quantityOnHand.GetValueOrDefault() == stockQuantity ? !quantityOnHand.HasValue : true))
                                {
                                    quantityOnHand = Convert.ToDecimal(item.QuantityOnHand.Value);
                                    productBySku.StockQuantity = (int)quantityOnHand;
                                    await _productService.UpdateProductAsync(productBySku);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception )
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroProduct"></param>
        public async Task UpdateXeroProduct(XeroProduct xeroProduct)
        {
           await  _xeroProductRepository.UpdateAsync(xeroProduct);
        }

        #endregion
    }
}