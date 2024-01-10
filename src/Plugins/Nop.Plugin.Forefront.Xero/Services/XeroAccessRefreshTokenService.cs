using Nop.Core.Domain.Logging;
using Nop.Core.Events;
using Nop.Data;
using Nop.Plugin.Forefront.Xero.Domain;
using Nop.Services.Events;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Models;
using Xero.NetStandard.OAuth2.Token;

namespace Nop.Plugin.Forefront.Xero.Services
{
	/// <summary>
	/// Xero Access Refresh Token Service
	/// </summary>
	public partial class XeroAccessRefreshTokenService : IXeroAccessRefreshTokenService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<XeroAccessRefreshToken> _accessRefreshTokenRepository;
        private readonly ForeFrontXeroSetting _tmotionsXeroSetting;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public XeroAccessRefreshTokenService(IEventPublisher eventPublisher,
            IRepository<XeroAccessRefreshToken> accessRefreshTokenRepository,
            ForeFrontXeroSetting tmotionsXeroSetting,
            IHttpClientFactory httpClientFactory,
            ILogger logger)
        {
            _eventPublisher = eventPublisher;
            _accessRefreshTokenRepository = accessRefreshTokenRepository;
            _tmotionsXeroSetting = tmotionsXeroSetting;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets first access refresh token
        /// </summary>
        /// <returns>First Access Refresh Token</returns>
        public async Task<XeroAccessRefreshToken> GetFirstAccessRefreshToken()
        {
            var query = _accessRefreshTokenRepository.Table;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Inserts Access Refresh Token
        /// </summary>
        /// <param name="accessRefreshToken">Access Refresh Token</param>
        public async Task InsertAccessRefreshToken(XeroAccessRefreshToken accessRefreshToken)
        {
            if (accessRefreshToken == null)
                throw new ArgumentNullException(nameof(accessRefreshToken));

           await _accessRefreshTokenRepository.InsertAsync(accessRefreshToken);

            //event notification
          await  _eventPublisher.EntityInsertedAsync(accessRefreshToken);
        }

        /// <summary>
        /// Updates the Access Refresh Token
        /// </summary>
        /// <param name="accessRefreshToken">Access Refresh Token</param>
        public async Task UpdateAccessRefreshToken(XeroAccessRefreshToken accessRefreshToken)
        {
            if (accessRefreshToken == null)
                throw new ArgumentNullException(nameof(accessRefreshToken));

           await _accessRefreshTokenRepository.UpdateAsync(accessRefreshToken);

            //event notification
           await _eventPublisher.EntityUpdatedAsync(accessRefreshToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IXeroToken> CheckRecordExist()
        {
            var record = await GetFirstAccessRefreshToken();
            if (record != null)
            {
                IXeroToken _xeroToken = new XeroOAuth2Token();
                _xeroToken.AccessToken = record.AccessToken;
                _xeroToken.RefreshToken = record.RefreshToken;
                _xeroToken.IdToken = record.IdentityToken;
                _xeroToken.ExpiresAtUtc = record.ExpiresIn;

                var tennat = new Tenant();
                tennat.id = (Guid)record.Tenant_Id;
                tennat.TenantId = (Guid)record.Tenant_TenantId;
                tennat.TenantType = record.Tenant_TenantType;
                tennat.CreatedDateUtc = (DateTime)record.Tenant_CreatedDateUtc;
                tennat.UpdatedDateUtc = (DateTime)record.Tenant_UpdatedDateUtc;
                _xeroToken.Tenants = new List<Tenant>();
                _xeroToken.Tenants.Add(tennat);

                var configuration = new XeroConfiguration();
                configuration.ClientId = _tmotionsXeroSetting.ClientId;
                configuration.ClientSecret = _tmotionsXeroSetting.ClientSecret;
                IXeroClient _xeroClient = new XeroClient(configuration, new HttpClient());

                if (_xeroToken.ExpiresAtUtc.Subtract(DateTime.UtcNow).Minutes <= 5)
                {
                    var _xeroTokenNew =    _xeroClient.RefreshAccessTokenAsync(_xeroToken).GetAwaiter();
                    _xeroToken = _xeroTokenNew.GetResult();

                    record.AccessToken = _xeroToken.AccessToken;
                    record.RefreshToken = _xeroToken.RefreshToken;
                    record.IdentityToken = _xeroToken.IdToken;
                    record.ExpiresIn = _xeroToken.ExpiresAtUtc;
                    record.Tenant_Id = _xeroToken.Tenants != null && _xeroToken.Tenants.Count > 0 ? _xeroToken.Tenants[0].id : (Guid?)null;
                    record.Tenant_TenantId = _xeroToken.Tenants != null && _xeroToken.Tenants.Count > 0 ? _xeroToken.Tenants[0].TenantId : (Guid?)null;
                    record.Tenant_TenantType = _xeroToken.Tenants != null && _xeroToken.Tenants.Count > 0 ? _xeroToken.Tenants[0].TenantType : string.Empty;
                    record.Tenant_CreatedDateUtc = _xeroToken.Tenants != null && _xeroToken.Tenants.Count > 0 ? (DateTime?)_xeroToken.Tenants[0].CreatedDateUtc : null;
                    record.Tenant_UpdatedDateUtc = _xeroToken.Tenants != null && _xeroToken.Tenants.Count > 0 ? (DateTime?)_xeroToken.Tenants[0].UpdatedDateUtc : null;

                    await _accessRefreshTokenRepository.UpdateAsync(record);

                    return _xeroToken;
                }
                else
                {
                    return _xeroToken;
                }
            }
            else
            {
               await _logger.InsertLogAsync(LogLevel.Error, "Please Setup and Connect Xoauth", "Please Setup and Connect Xoauth", null);
                return null;
            }
        }

        #endregion
    }
}