using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Models;

namespace Nop.Plugin.Api.Factories
{
    public interface IVendorModelFactory
    {
        /// <summary>
        /// Prepare favorite vendor/store model
        /// </summary>
        /// <param name="favoriteModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<FavoriteVendorModel> PrepareFavoriteVendorModelAsync(FavoriteVendorModel favoriteModel, Customer customer);
    }
}
