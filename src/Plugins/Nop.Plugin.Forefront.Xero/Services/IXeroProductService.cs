using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Forefront.Xero.Domain;

namespace Nop.Plugin.Forefront.Xero.Services
{
    public interface IXeroProductService
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="isDeleted"></param>
        /// <param name="actionType"></param>
        /// <param name="xeroStatus"></param>
		Task ChangeXeroProductStatus(int productId, bool isDeleted, string actionType, bool xeroStatus = false);

        /// <summary>
        ///  Create and Update Product in xero
        /// </summary>
        /// <param name="products"></param>
		Task CreateXeroItem(IList<Product> products);

        /// <summary>
        /// While Create Order insert and Update Product in xero
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        Task CreateXeroItemOrderTime(IList<Product> products);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
		Task<IPagedList<XeroProduct>> GetAllXeroProduct(int pageIndex, int pageSize);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		Task<IList<XeroProduct>> GetProductsForInventory();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		Task<IList<XeroProduct>> GetProductsForInventoryByFixedBatch();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
		Task<XeroProduct> GetXeroProductByProductId(int productId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroProduct"></param>
		Task InsertXeroProduct(XeroProduct xeroProduct);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="actionType"></param>
		Task InsertXeroProductByProductId(int productId, string actionType);

        /// <summary>
        /// 
        /// </summary>
		Task ManageProductQuantity();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroProduct"></param>
		Task UpdateXeroProduct(XeroProduct xeroProduct);
	}
}