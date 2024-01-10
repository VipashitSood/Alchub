using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Web.Models.Favorite;

namespace Nop.Web.Alchub.Factories
{
    public partial interface IFavoriteModelFactory
    {
        /// <summary>
        /// Prepare favorite model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="customer"></param>
        /// <param name="isEditable"></param>
        /// <returns></returns>
        Task<FavoriteModel> PrepareFavoriteModelAsync(FavoriteModel model, Customer customer, bool isEditable = true);

        /// <summary>
        /// Prepare favorite vendor/store model
        /// </summary>
        /// <param name="favoriteModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<FavoriteModel> PrepareFavoriteVendorModelAsync(FavoriteModel favoriteModel, Customer customer);
    }
}
