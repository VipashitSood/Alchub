using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Plugin.Forefront.Xero.Domain;
using Nop.Plugin.Forefront.Xero.Models;
using Nop.Plugin.Forefront.Xero.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Model.Accounting;
using static Nop.Plugin.Forefront.Xero.Models.ForeFrontXeroModel;

namespace Nop.Plugin.Forefront.Xero.Controllers
{
    public class ForefrontXeroController : BasePluginController
    {
		#region Fields

		private readonly IPermissionService _permissionService;
		private readonly ISettingService _settingService;
		private readonly ILocalizationService _localizationService;
		private readonly IStoreService _storeService;
		private readonly IWorkContext _workContext;
		private readonly ICustomerService _customerService;
		private readonly IXeroQueueService _xeroQueueService;
		private readonly ILogger _logger;
		private readonly IScheduleTaskService _scheduleTaskService;
		private readonly IWebHelper _webHelper;
		private readonly IPaymentPluginManager _paymentPluginManager;
		private readonly IStoreContext _storeContext;
		private readonly IXeroProductService _xeroProductService;
		private readonly INopFileProvider _nopFileProvider;
		private readonly INotificationService _notificationService;
		private readonly IXeroAccessRefreshTokenService _xeroAccessRefreshTokenService;

		#endregion

		#region Ctor

		public ForefrontXeroController(IPermissionService permissionService, ISettingService settingService,
			ILocalizationService localizationService, IStoreService storeService, IWorkContext workContext,
			ICustomerService customerService, IXeroQueueService xeroQueueService,
			ILogger logger, IScheduleTaskService scheduleTaskService, IWebHelper webHelper, IPaymentPluginManager paymentPluginManager,
			IStoreContext storeContext, IXeroProductService xeroProductService,
			INopFileProvider nopFileProvider, INotificationService notificationService,
			IXeroAccessRefreshTokenService xeroAccessRefreshTokenService)
		{
			this._permissionService = permissionService;
			this._settingService = settingService;
			this._localizationService = localizationService;
			this._storeService = storeService;
			this._workContext = workContext;
			this._customerService = customerService;
			this._xeroQueueService = xeroQueueService;
			this._logger = logger;
			this._scheduleTaskService = scheduleTaskService;
			this._webHelper = webHelper;
			this._paymentPluginManager = paymentPluginManager;
			this._storeContext = storeContext;
			this._xeroProductService = xeroProductService;
			this._nopFileProvider = nopFileProvider;
			this._notificationService = notificationService;
			this._xeroAccessRefreshTokenService = xeroAccessRefreshTokenService;
		}

		#endregion

		#region Methods

		/// <summary>
		/// When you authorize the xero account from admin configure screen the request came back on this function
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		[HttpGet("/callback")]
        public async Task<ContentResult> CallBackURL(string code)
        {
            int activeStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var setting = await _settingService.LoadSettingAsync<ForeFrontXeroSetting>(activeStoreScopeConfiguration);

			var client = new HttpClient();
			var result = new ContentResult();

			var response = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
			{
				Address = setting.TokenURL,
				GrantType = setting.TokenGrantType,
				Code = code,
				ClientId = setting.ClientId,
				ClientSecret = setting.ClientSecret,
				RedirectUri = setting.CallbackURL,
				Parameters =
					{
						{ "scope", setting.Scope }
					}
			});

			if (response.IsError)
			{
				await _logger.InsertLogAsync(LogLevel.Error, response.Error, response.Error);
				return result;
			}

			string tenant;
			var tenantList = new List<TenantModel>();
			var accessToken = response.AccessToken;
			var refreshToken = response.RefreshToken;
			var identityToken = response.IdentityToken;

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response.AccessToken);
			using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, setting.ConnectionsURL))
			{
				var httpResult = client.SendAsync(requestMessage).Result;
				tenant = httpResult.Content.ReadAsStringAsync().Result;
				tenantList = JsonConvert.DeserializeObject<List<TenantModel>>(tenant);
			}

			var _xeroAccessRefreshTokenService = EngineContext.Current.Resolve<IXeroAccessRefreshTokenService>();
			var record = await _xeroAccessRefreshTokenService.GetFirstAccessRefreshToken();
			if (record == null)
			{
				var obj = new XeroAccessRefreshToken
				{
					AccessToken = response.AccessToken,
					RefreshToken = response.RefreshToken,
					IdentityToken = response.IdentityToken,
					ExpiresIn = DateTime.UtcNow.AddSeconds(response.ExpiresIn),
					Tenant_Id = tenantList != null && tenantList.Count > 0 ? tenantList[0].id : (Guid?)null,
					Tenant_TenantId = tenantList != null && tenantList.Count > 0 ? tenantList[0].TenantId : (Guid?)null,
					Tenant_TenantType = tenantList != null && tenantList.Count > 0 ? tenantList[0].TenantType : string.Empty,
					Tenant_CreatedDateUtc = tenantList != null && tenantList.Count > 0 ? tenantList[0].Tenant_CreatedDateUtc : null,
					Tenant_UpdatedDateUtc = tenantList != null && tenantList.Count > 0 ? tenantList[0].Tenant_UpdatedDateUtc : null,
				};

				await _xeroAccessRefreshTokenService.InsertAccessRefreshToken(obj);
			}
			else
			{
				record.AccessToken = response.AccessToken;
				record.RefreshToken = response.RefreshToken;
				record.IdentityToken = response.IdentityToken;
				record.ExpiresIn = DateTime.UtcNow.AddSeconds(response.ExpiresIn);
				record.Tenant_Id = tenantList != null && tenantList.Count > 0 ? tenantList[0].id : (Guid?)null;
				record.Tenant_TenantId = tenantList != null && tenantList.Count > 0 ? tenantList[0].TenantId : (Guid?)null;
				record.Tenant_TenantType = tenantList != null && tenantList.Count > 0 ? tenantList[0].TenantType : string.Empty;
				record.Tenant_CreatedDateUtc = tenantList != null && tenantList.Count > 0 ? tenantList[0].Tenant_CreatedDateUtc : null;
				record.Tenant_UpdatedDateUtc = tenantList != null && tenantList.Count > 0 ? tenantList[0].Tenant_UpdatedDateUtc : null;

				await _xeroAccessRefreshTokenService.UpdateAccessRefreshToken(record);
			}

			var content = String.Format(@"<html><head></head><body>
                        <h3>AccessToken</h3><p>{0}</p>
                        <h3>RefreshToken</h3><p>{1}</p>
                        <h3>IdentityToken</h3><p>{2}</p>
                        <h3>Tenant</h3><p>{3}</p>
                        </body></html>", accessToken, refreshToken, identityToken, tenant);

			result.Content = content;
			result.ContentType = "text/html";

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        [Area("Admin")]
        [AuthorizeAdmin(false)]
        public async Task<IActionResult> Configure()
        {
            IActionResult route;
            int activeStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var setting = await _settingService.LoadSettingAsync<ForeFrontXeroSetting>(activeStoreScopeConfiguration);

            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            {
                var model = new ForeFrontXeroModel()
                {
					ActiveStoreScopeConfiguration = activeStoreScopeConfiguration,
					Enable = setting.Enable,
					IsDropTable = setting.IsDropTable,
					IsInventorySyncEnabled = setting.IsInventorySyncEnabled,
					ClientId = setting.ClientId,
					ClientSecret = setting.ClientSecret,
					CallbackURL = setting.CallbackURL,
					SalesAccountCode = setting.SalesAccountCode,
					PurchaseAccountCode = setting.PurchaseAccountCode,
					NonVatAccountCode = setting.NonVatAccountCode,
					PostageAccountCode = setting.PostageAccountCode,
					DiscountAmountAccountCode = setting.DiscountAmountAccountCode,
					PaymentMethodAdditonalFeeAccountCode = setting.PaymentMethodAdditonalFeeAccountCode,
					RewardPointsAccountCode = setting.RewardPointsAccountCode,
					BatchSize = setting.BatchSize,
					AuthorizedURL = setting.AuthorizedURL,
					AuthorizedResponseType = setting.AuthorizedResponseType,
					AuthorizedState = setting.AuthorizedState,
					TokenURL = setting.TokenURL,
					TokenGrantType = setting.TokenGrantType,
					Scope = setting.Scope,
					ConnectionsURL = setting.ConnectionsURL
				};

                try
                {
                    PrepareAccountLiabilityModel(model);
                }
                catch (Exception ex)
                {
                  await _logger.InsertLogAsync(LogLevel.Error, string.Concat("Xero Account liability ", ex.InnerException), string.Concat(ex.Message, "stacktrace", ex.StackTrace), null);
                }

                if (activeStoreScopeConfiguration > 0)
                {
					model.Enable_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, bool>(setting, (ForeFrontXeroSetting x) => x.Enable, activeStoreScopeConfiguration);
					model.IsDropTable_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, bool>(setting, (ForeFrontXeroSetting x) => x.IsDropTable, activeStoreScopeConfiguration);
					model.IsInventorySyncEnabled_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, bool>(setting, (ForeFrontXeroSetting x) => x.IsInventorySyncEnabled, activeStoreScopeConfiguration);
					model.ClientId_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.ClientId, activeStoreScopeConfiguration);
					model.ClientSecret_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.ClientSecret, activeStoreScopeConfiguration);
					model.CallbackURL_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.CallbackURL, activeStoreScopeConfiguration);
					model.SalesAccountCode_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.SalesAccountCode, activeStoreScopeConfiguration);
					model.PurchaseAccountCode_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.PurchaseAccountCode, activeStoreScopeConfiguration);
					model.NonVatAccountCode_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.NonVatAccountCode, activeStoreScopeConfiguration);
					model.PostageAccountCode_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.PostageAccountCode, activeStoreScopeConfiguration);
					model.DiscountAmountAccountCode_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.DiscountAmountAccountCode, activeStoreScopeConfiguration);
					model.PaymentMethodAdditonalFeeAccountCode_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.PaymentMethodAdditonalFeeAccountCode, activeStoreScopeConfiguration);
					model.RewardPointsAccountCode_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.RewardPointsAccountCode, activeStoreScopeConfiguration);
					model.BatchSize_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, int>(setting, (ForeFrontXeroSetting x) => x.BatchSize, activeStoreScopeConfiguration);
					model.AuthorizedURL_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.AuthorizedURL, activeStoreScopeConfiguration);
					model.AuthorizedResponseType_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.AuthorizedResponseType, activeStoreScopeConfiguration);
					model.AuthorizedState_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.AuthorizedState, activeStoreScopeConfiguration);
					model.TokenURL_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.TokenURL, activeStoreScopeConfiguration);
					model.TokenGrantType_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.TokenGrantType, activeStoreScopeConfiguration);
					model.Scope_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.Scope, activeStoreScopeConfiguration);
					model.ConnectionsURL_OverrideForStore = await _settingService.SettingExistsAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.ConnectionsURL, activeStoreScopeConfiguration);
				}

				if (!String.IsNullOrWhiteSpace(setting.AuthorizedURL))
				{
					var xeroAuthorizeUri = new RequestUrl(setting.AuthorizedURL);
					model.ConnectToXero = xeroAuthorizeUri.CreateAuthorizeUrl(
							clientId: setting.ClientId,
							responseType: setting.AuthorizedResponseType,
							redirectUri: setting.CallbackURL,
							state: setting.AuthorizedState,
							scope: setting.Scope);
				}

				route = View("~/Plugins/Forefront.Xero/Views/Configure.cshtml", model);
            }
            else
            {
                route = AccessDeniedView();
            }

            return route;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
        [ActionName("Configure")]
        [Area("Admin")]
        [AuthorizeAdmin(false)]
        [FormValueRequired(new string[] { "save" })]
        [HttpPost]
        public async Task<IActionResult> Configure(ForeFrontXeroModel model)
        {
            IActionResult actionResult;
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            {
                actionResult = AccessDeniedView();
            }
            else if (ModelState.IsValid)
            {
                int activeStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var setting =await _settingService.LoadSettingAsync<ForeFrontXeroSetting>(activeStoreScopeConfiguration);

				setting.Enable = model.Enable;
				setting.IsDropTable = model.IsDropTable;
				setting.IsInventorySyncEnabled = model.IsInventorySyncEnabled;
				setting.ClientId = model.ClientId;
				setting.ClientSecret = model.ClientSecret;
				setting.CallbackURL = model.CallbackURL;
				setting.SalesAccountCode = model.SalesAccountCode;
				setting.PurchaseAccountCode = model.PurchaseAccountCode;
				setting.NonVatAccountCode = model.NonVatAccountCode;
				setting.PostageAccountCode = model.PostageAccountCode;
				setting.DiscountAmountAccountCode = model.DiscountAmountAccountCode;
				setting.PaymentMethodAdditonalFeeAccountCode = model.PaymentMethodAdditonalFeeAccountCode;
				setting.RewardPointsAccountCode = model.RewardPointsAccountCode;
				setting.BatchSize = model.BatchSize;
				setting.AuthorizedURL = model.AuthorizedURL;
				setting.AuthorizedResponseType = model.AuthorizedResponseType;
				setting.AuthorizedState = model.AuthorizedState;
				setting.TokenURL = model.TokenURL;
				setting.TokenGrantType = model.TokenGrantType;
				setting.Scope = model.Scope;
				setting.ConnectionsURL = model.ConnectionsURL;

				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, bool>(setting, (ForeFrontXeroSetting x) => x.Enable, model.Enable_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, bool>(setting, (ForeFrontXeroSetting x) => x.IsInventorySyncEnabled, model.IsInventorySyncEnabled_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.ClientId, model.ClientId_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.ClientSecret, model.ClientSecret_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.CallbackURL, model.CallbackURL_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, bool>(setting, (ForeFrontXeroSetting x) => x.IsDropTable, model.IsDropTable_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.SalesAccountCode, model.SalesAccountCode_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.PurchaseAccountCode, model.PurchaseAccountCode_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.NonVatAccountCode, model.NonVatAccountCode_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.PostageAccountCode, model.PostageAccountCode_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.DiscountAmountAccountCode, model.DiscountAmountAccountCode_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.PaymentMethodAdditonalFeeAccountCode, model.PaymentMethodAdditonalFeeAccountCode_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.RewardPointsAccountCode, model.RewardPointsAccountCode_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, int>(setting, (ForeFrontXeroSetting x) => x.BatchSize, model.BatchSize_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.AuthorizedURL, model.AuthorizedURL_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.AuthorizedResponseType, model.AuthorizedResponseType_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.AuthorizedResponseType, model.AuthorizedState_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.TokenURL, model.TokenURL_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.TokenGrantType, model.TokenGrantType_OverrideForStore, activeStoreScopeConfiguration, false);
				await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.Scope, model.Scope_OverrideForStore, activeStoreScopeConfiguration, false);
                await _settingService.SaveSettingOverridablePerStoreAsync<ForeFrontXeroSetting, string>(setting, (ForeFrontXeroSetting x) => x.ConnectionsURL, model.ConnectionsURL_OverrideForStore, activeStoreScopeConfiguration, false);

				_notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"), true);

				await SaveSelectedTabName("tab-info", true);

				await _settingService.ClearCacheAsync();

				actionResult = await Configure();
            }
            else
            {
                actionResult = await Configure();
            }

            return actionResult;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
        [Area("Admin")]
        [AuthorizeAdmin(false)]
        [HttpPost]
        public async Task<IActionResult> DeleteXeroQueue(int id)
        {
            var queueById = await _xeroQueueService.GetQueueById(id);
           await  _xeroQueueService.DeleteQueue(queueById);
            return Json(queueById);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ManagePayment()
        {
           await _xeroQueueService.ManagePayment();
            return Json("success");
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        [Area("Admin")]
        [AuthorizeAdmin(false)]
        public async Task<IActionResult> MiscConfigure()
        {
            return View("~/Plugins/Forefront.Xero/Views/MiscConfigure.cshtml");
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="foreFrontXeroModel"></param>
		public async Task PrepareAccountLiabilityModel(ForeFrontXeroModel foreFrontXeroModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var activePlugins = await _paymentPluginManager.LoadActivePluginsAsync(customer, store.Id, 0);
            foreach (var paymentMethod in activePlugins)
            {
                if (paymentMethod.PluginDescriptor != null)
                {
                    var model = new ForeFrontXeroModel.AccountLiabilityModel()
                    {
                        NopPaymentMethodName = string.Concat(paymentMethod.PluginDescriptor.FriendlyName, " ( ", paymentMethod.PluginDescriptor.SystemName, " )")
                    };

					try
					{
						var record = await _xeroAccessRefreshTokenService.CheckRecordExist();
						if (record != null)
						{
							IAccountingApi accountingApi = new AccountingApi();

							model.AvailablePaymentMethod.Add(new SelectListItem { Text = "--Select Account--", Value = "0" });

							var accountsApi = accountingApi.GetAccountsAsync(record.AccessToken, record.Tenants[0].TenantId.ToString()).GetAwaiter();
							IList<Account> accounts = accountsApi.GetResult()._Accounts;
							accounts = accounts.Where(x => (x.EnablePaymentsToAccount.GetValueOrDefault() ? x.EnablePaymentsToAccount.HasValue : false) ? true : x.Type == AccountType.BANK).ToList();

							foreach (var account in accounts)
							{
								model.AvailablePaymentMethod.Add(new SelectListItem { Text = account.Name, Value = account.AccountID.ToString() });
							}
						}
					}
					catch (Exception ex)
					{
						await _logger.InsertLogAsync(LogLevel.Error, string.Concat("Xero Account sync", ex.InnerException), string.Concat(ex.Message, "stacktrace", ex.StackTrace), null);
					}

					var xeroAccountByPaymentMethodSystemName = await _xeroQueueService.GetXeroAccountByPaymentMethodSystemName(paymentMethod.PluginDescriptor.SystemName);
                    if (xeroAccountByPaymentMethodSystemName != null)
                    {
                        var selectListItem = model.AvailablePaymentMethod.FirstOrDefault(x => x.Value == xeroAccountByPaymentMethodSystemName.XeroAccountId);
                        if (selectListItem != null)
                        {
                            selectListItem.Selected = true;
                        }
                    }

                    model.PaymentMethodSystemName = paymentMethod.PluginDescriptor.SystemName;
                    foreFrontXeroModel.AccountLiabilityModels.Add(model);
                }
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RunScheduleTask(int id)
        {
            var taskByType = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Forefront.Xero.ScheduleTasks.XeroIntegration");
            if (taskByType != null)
            {
                taskByType.Enabled = true;
            }

            return Json("success");
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="model"></param>
		/// <param name="form"></param>
		/// <returns></returns>
        [ActionName("SavePaymentinformation")]
        [Area("Admin")]
        [AuthorizeAdmin(false)]
        [HttpPost]
        public async Task<IActionResult> SavePaymentinformation(ForeFrontXeroModel.AccountLiabilityModel model, IFormCollection form)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var activePlugins = await _paymentPluginManager.LoadActivePluginsAsync(customer, store.Id, 0);
            foreach (var paymentMethod in activePlugins)
            {
                if (paymentMethod.PluginDescriptor != null)
                {
                    if (!string.IsNullOrEmpty(paymentMethod.PluginDescriptor.SystemName))
                    {
                        string str = string.Concat("Account_", paymentMethod.PluginDescriptor.SystemName);
                        var item = form[str];
                        if (!string.IsNullOrEmpty(item))
                        {
                            var xeroAccountByPaymentMethodSystemName = await _xeroQueueService.GetXeroAccountByPaymentMethodSystemName(paymentMethod.PluginDescriptor.SystemName);
                            if (xeroAccountByPaymentMethodSystemName != null)
                            {
                                xeroAccountByPaymentMethodSystemName.XeroAccountId = item;
                               await _xeroQueueService.UpdateAccountMap(xeroAccountByPaymentMethodSystemName);
                            }
                            else
                            {
                                var xeroAccounting = new XeroAccounting()
                                {
                                    NopPaymentMethod = paymentMethod.PluginDescriptor.SystemName,
                                    XeroAccountId = item
                                };

                               await _xeroQueueService.InsertAccountMap(xeroAccounting);
                            }
                        }
                    }
                }
            }

            await SaveSelectedTabName("tab-AccountLiability", true);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Account.saved"), true);

            IActionResult actionResult = Redirect(string.Format("{0}Admin/ForefrontXero/Configure", _webHelper.GetStoreLocation()));

            return actionResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabName"></param>
        /// <param name="persistForTheNextRequest"></param>
        protected virtual async Task SaveSelectedTabName(string tabName = "", bool persistForTheNextRequest = true)
        {
            SaveSelectedTabName(tabName, "selected-tab-name", null, persistForTheNextRequest);
            if (base.Request.Method == "POST")
            {
                foreach (string key in base.Request.Form.Keys)
                {
                    if (!key.StartsWith("selected-tab-name-"))
                    {
                        continue;
                    }

                    SaveSelectedTabName(null, key, key.Substring("selected-tab-name-".Length), persistForTheNextRequest);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabName"></param>
        /// <param name="formKey"></param>
        /// <param name="dataKeyPrefix"></param>
        /// <param name="persistForTheNextRequest"></param>
        protected virtual void SaveSelectedTabName(string tabName, string formKey, string dataKeyPrefix, bool persistForTheNextRequest)
        {
            if (string.IsNullOrEmpty(tabName))
            {
                tabName = formKey;
            }

            if (!string.IsNullOrEmpty(tabName))
            {
                string str = "nop.selected-tab-name";
                if (!string.IsNullOrEmpty(dataKeyPrefix))
                {
                    str = string.Concat(str, string.Format("-{0}", dataKeyPrefix));
                }
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        [Area("Admin")]
        [AuthorizeAdmin(false)]
        public IActionResult XeroProductList()
        {
            IActionResult actionResult;
            if (_workContext.GetCurrentVendorAsync() == null)
            {
                actionResult = View("~/Plugins/ForeFront.Plugin.Algolia.Core/Views/AlgoliaAdmin/IncerementalProducts.cshtml", new ForeFrontXeroModel.XeroProductModel());
            }
            else
            {
                actionResult = AccessDeniedView();
            }

            return actionResult;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="searchModel"></param>
		/// <returns></returns>
        [Area("Admin")]
        [AuthorizeAdmin(false)]
        [HttpPost]
        public async Task<IActionResult> XeroProductList(ForeFrontXeroSearchModel searchModel)
        {
            var allXeroProduct = await _xeroProductService.GetAllXeroProduct(searchModel.Page - 1, searchModel.PageSize);

            var model = new ForeFrontXeroModel.XeroProductListModel().PrepareToGrid(searchModel, allXeroProduct, () =>
            {
                return allXeroProduct.Select(product =>
                {
					XeroProductModel xeroProductModel = new XeroProductModel();

                    xeroProductModel.Id = product.Id;
                    xeroProductModel.ProductId = product.ProductId;
                    xeroProductModel.XeroStatus = (product.XeroStatus ? "Indexed" : "Pending");
                    xeroProductModel.IsDeleted = product.IsDeleted;
                    xeroProductModel.ActionType = product.ActionType;

                    return xeroProductModel;
                });
            });

            return Json(model);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="searchModel"></param>
		/// <returns></returns>
        [Area("Admin")]
        [AuthorizeAdmin(false)]
        [HttpPost]
        public virtual async Task<IActionResult> XeroQueueList(ForeFrontXeroSearchModel searchModel)
        {
            var pagedList = await _xeroQueueService.AllXeroQueue(searchModel.Page - 1, searchModel.PageSize);

            var queueListModel = new ForeFrontXeroModel.QueueListModel().PrepareToGrid(searchModel, pagedList, () =>
            {
                return pagedList.Select(xeroqueue =>
                {
					ForeFrontQueueModel model =new ForeFrontQueueModel();

					model.Id = xeroqueue.Id;
					model.OrderId = xeroqueue.OrderId;
					model.QueuedOn = xeroqueue.QueuedOn;
					model.SyncAttemptOn = xeroqueue.SyncAttemptOn;
					model.Amount = xeroqueue.Amount;
					model.ActionType = xeroqueue.ActionType;
					model.IsSuccess = xeroqueue.IsSuccess;
					model.SuccessMessage = (xeroqueue.IsSuccess ? "success" : "waiting");

                    return model;
                });
            });

            return Json(queueListModel);
        }

        #endregion
    }
}