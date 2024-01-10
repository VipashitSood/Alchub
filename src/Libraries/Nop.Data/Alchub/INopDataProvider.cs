using System.Threading.Tasks;

namespace Nop.Data
{
    /// <summary>
    /// Represents extended a data provider
    /// </summary>
    public partial interface INopDataProvider
    {
        #region Methods

        /// <summary>
        /// Execute sql commands
        /// </summary>
        Task ExecuteSqlFileScriptsOnDatabase();

        #endregion
    }
}