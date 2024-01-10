using System.Threading.Tasks;

namespace Nop.Services.Alchub.ElasticSearch
{
    public partial interface IElasticsearchIndexCreator
    {
        Task CreateElasticsearchIndex();

        Task<bool> DeleteProductDocumentAsync(int productId);
    }
}
