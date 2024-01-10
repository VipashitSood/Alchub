using System.Threading.Tasks;
using Nop.Plugin.Forefront.Xero.Domain;
using Xero.NetStandard.OAuth2.Token;

namespace Nop.Plugin.Forefront.Xero.Services
{
    /// <summary>
    /// Xero Access Refresh Token Service Interface
    /// </summary>
    public partial interface IXeroAccessRefreshTokenService
    {
        /// <summary>
        /// Gets first access refresh token
        /// </summary>
        /// <returns>First Access Refresh Token</returns>
        Task<XeroAccessRefreshToken> GetFirstAccessRefreshToken();

        /// <summary>
        /// Inserts Access Refresh Token
        /// </summary>
        /// <param name="accessRefreshToken">Access Refresh Token</param>
        Task InsertAccessRefreshToken(XeroAccessRefreshToken accessRefreshToken);

        /// <summary>
        /// Updates the Access Refresh Token
        /// </summary>
        /// <param name="accessRefreshToken">Access Refresh Token</param>
        Task UpdateAccessRefreshToken(XeroAccessRefreshToken accessRefreshToken);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IXeroToken> CheckRecordExist();
    }
}