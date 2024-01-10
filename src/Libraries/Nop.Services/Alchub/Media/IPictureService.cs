using System.Threading.Tasks;
using Nop.Core.Domain.Media;

namespace Nop.Services.Media
{
    public partial interface IPictureService
    {
        Task<string> GetProductPictureUrlAsync(string upcCode,
            int targetSize = 0,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity);
    }
}
