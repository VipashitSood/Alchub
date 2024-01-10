using System.Threading.Tasks;

namespace Nop.Services.Alchub.ServiceFee
{
    /// <summary>
    /// Represents a service fee manager service
    /// </summary>
    public partial interface IServiceFeeManager
    {
        /// <summary>
        /// Gets service fee
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order model
        /// </returns>
        Task<decimal> GetServiceFeeAsync(decimal subtotal);
    }
}
