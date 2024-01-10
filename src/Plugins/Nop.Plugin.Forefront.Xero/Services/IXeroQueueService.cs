using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Forefront.Xero.Domain;

namespace Nop.Plugin.Forefront.Xero.Services
{
    public interface IXeroQueueService
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
         Task<IPagedList<XeroQueue>> AllXeroQueue(int pageIndex = 0, int pageSize = 2147483647);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroQueue"></param>
         Task CancelInvoice(IList<XeroQueue> xeroQueue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroQueue"></param>
		Task CreateXeroInvoice(IList<XeroQueue> xeroQueue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="XeroAccounting"></param>
		Task DeleteAccountMap(XeroAccounting xeroAccounting);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
		Task DeleteQueue(XeroQueue queue);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		Task<IList<XeroQueue>> GetAllQueue();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		Task<IList<XeroQueue>> GetAllXeroQueueByOrderIdAndXeroId();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
		Task<XeroQueue> GetParentQueueByOrderId(int orderId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
		Task<XeroQueue> GetPaymentQueueByOrderId(int orderId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		Task<XeroQueue> GetQueueById(int id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
		Task<XeroQueue> GetQueueByOrderId(int orderId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		Task<IList<XeroQueue>> GetQueueForCancelOrder();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
		Task<IList<XeroQueue>> GetQueueForInvoice(int num = 0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
		Task<IList<XeroQueue>> GetQueueForPayment(int num = 0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
		Task<XeroQueue> GetUnPaidInvoice(string invoiceId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SystemName"></param>
        /// <returns></returns>
		Task<XeroAccounting> GetXeroAccountByPaymentMethodSystemName(string systemName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="XeroAccounting"></param>
		Task InsertAccountMap(XeroAccounting xeroAccounting);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
		Task InsertQueue(XeroQueue queue);

        /// <summary>
        /// 
        /// </summary>
		Task ManagePayment();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="XeroAccounting"></param>
		Task UpdateAccountMap(XeroAccounting xeroAccounting);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
		Task UpdateQueue(XeroQueue queue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroQueue"></param>
		Task XeroPayment(IList<XeroQueue> xeroQueue);
	}
}