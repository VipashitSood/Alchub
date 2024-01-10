using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Forefront.Xero.Models
{
	public record ForeFrontXeroModel : BaseNopModel
    {
		public IList<ForeFrontXeroModel.AccountLiabilityModel> AccountLiabilityModels
		{
			get;
			set;
		}

		public IList<SelectListItem> AvailableAccounts
		{
			get;
			set;
		}

		public IList<SelectListItem> AvailableStores
		{
			get;
			set;
		}

		public int ActiveStoreScopeConfiguration
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.Enable")]
		public bool Enable
		{
			get;
			set;
		}

		public bool Enable_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.IsDropTable")]
		public bool IsDropTable
		{
			get;
			set;
		}

		public bool IsDropTable_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.IsInventorySyncEnabled")]
		public bool IsInventorySyncEnabled
		{
			get;
			set;
		}

		public bool IsInventorySyncEnabled_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.ClientId")]
		public string ClientId
		{
			get;
			set;
		}

		public bool ClientId_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.ClientSecret")]
		public string ClientSecret
		{
			get;
			set;
		}

		public bool ClientSecret_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.CallbackURL")]
		public string CallbackURL
		{
			get;
			set;
		}

		public bool CallbackURL_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.ConnectToXero")]
		public string ConnectToXero
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.SalesAccountCode")]
		public string SalesAccountCode
		{
			get;
			set;
		}

		public bool SalesAccountCode_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.NonVatAccountCode")]
		public string NonVatAccountCode
		{
			get;
			set;
		}

		public bool NonVatAccountCode_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.PurchaseAccountCode")]
		public string PurchaseAccountCode
		{
			get;
			set;
		}

		public bool PurchaseAccountCode_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.PostageAccountCode")]
		public string PostageAccountCode
		{
			get;
			set;
		}

		public bool PostageAccountCode_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.DiscountAmountAccountCode")]
		public string DiscountAmountAccountCode
		{
			get;
			set;
		}

		public bool DiscountAmountAccountCode_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.RewardPointsAccountCode")]
		public string RewardPointsAccountCode
		{
			get;
			set;
		}
		public bool RewardPointsAccountCode_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.BatchSize")]
		public int BatchSize
		{
			get;
			set;
		}

		public bool BatchSize_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.AuthorizedURL")]
		public string AuthorizedURL
		{
			get;
			set;
		}

		public bool AuthorizedURL_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.AuthorizedResponseType")]
		public string AuthorizedResponseType
		{
			get;
			set;
		}

		public bool AuthorizedResponseType_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.AuthorizedState")]
		public string AuthorizedState
		{
			get;
			set;
		}

		public bool AuthorizedState_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.TokenURL")]
		public string TokenURL
		{
			get;
			set;
		}

		public bool TokenURL_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.TokenGrantType")]
		public string TokenGrantType
		{
			get;
			set;
		}

		public bool TokenGrantType_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.Scope")]
		public string Scope
		{
			get;
			set;
		}

		public bool Scope_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.ConnectionsURL")]
		public string ConnectionsURL
		{
			get;
			set;
		}

		public bool ConnectionsURL_OverrideForStore
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugin.Forefront.Xero.Fields.PaymentMethodAdditonalFeeAccountCode")]
		public string PaymentMethodAdditonalFeeAccountCode
		{
			get;
			set;
		}

		public bool PaymentMethodAdditonalFeeAccountCode_OverrideForStore
		{
			get;
			set;
		}

        public ForeFrontXeroSearchModel ForeFrontXeroSearchModel { get; set; }

        public ForeFrontXeroModel()
		{
			this.AvailableStores = new List<SelectListItem>();
			this.AvailableAccounts = new List<SelectListItem>();
			this.AccountLiabilityModels = new List<ForeFrontXeroModel.AccountLiabilityModel>();
            ForeFrontXeroSearchModel = new ForeFrontXeroSearchModel();
        }

		public record AccountLiabilityModel : BaseNopEntityModel
		{
			public IList<SelectListItem> AvailablePaymentMethod
			{
				get;
				set;
			}

			public string NopPaymentMethodName
			{
				get;
				set;
			}

			public string PaymentMethodSystemName
			{
				get;
				set;
			}

			public int PaymnetMethodId
			{
				get;
				set;
			}

			public AccountLiabilityModel()
			{
				this.AvailablePaymentMethod = new List<SelectListItem>();
			}
		}

		public record ForeFrontQueueModel : BaseNopEntityModel
		{
			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.ActionType")]
			public string ActionType
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.Amount")]
			public decimal Amount
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.IsSuccess")]
			public bool IsSuccess
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.OrderId")]
			public int OrderId
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.ParentId")]
			public int ParentId
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.QueuedOn")]
			public DateTime QueuedOn
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.ResponseData")]
			public string ResponseData
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.ResponseMessages")]
			public string ResponseMessages
			{
				get;
				set;
			}

			public string SuccessMessage
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.SyncAttemptCount")]
			public int SyncAttemptCount
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.SyncAttemptOn")]
			public DateTime? SyncAttemptOn
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugin.Forefront.Xero.Field.XeroId")]
			public string XeroId
			{
				get;
				set;
			}

			public ForeFrontQueueModel()
			{
			}
		}

		public record XeroProductModel : BaseNopEntityModel
		{
			[NopResourceDisplayName("Plugins.ForeFront.Algolia.Core.Fields.ActionType")]
			public string ActionType
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugins.ForeFront.Algolia.Core.Fields.IsDeleted")]
			public bool IsDeleted
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugins.ForeFront.Algolia.Core.Fields.IsDeletedId")]
			public int IsDeletedId
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugins.ForeFront.Algolia.Core.Fields.ProductId")]
			public int ProductId
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugins.ForeFront.Algolia.Core.Fields.XeroStatus")]
			public string XeroStatus
			{
				get;
				set;
			}

			[NopResourceDisplayName("Plugins.ForeFront.Algolia.Core.Fields.XeroStatusId")]
			public int XeroStatusId
			{
				get;
				set;
			}

			public XeroProductModel()
			{
			}
		}

        /// <summary>
        /// Represents a xero product list model
        /// </summary>
        public record XeroProductListModel : BasePagedListModel<XeroProductModel>
        {

        }

        /// <summary>
        /// Represents a queue list model
        /// </summary>
        public record QueueListModel : BasePagedListModel<ForeFrontQueueModel>
        {

        }
    }

    /// <summary>
    /// Represents a fore front xero search model
    /// </summary>
    public record ForeFrontXeroSearchModel : BaseSearchModel
    {

    }
}