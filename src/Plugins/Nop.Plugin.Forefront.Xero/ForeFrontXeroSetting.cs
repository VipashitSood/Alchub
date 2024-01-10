using Nop.Core.Configuration;

namespace Nop.Plugin.Forefront.Xero
{
	public class ForeFrontXeroSetting : ISettings
	{
		public ForeFrontXeroSetting()
		{
		}

		public int ActiveStoreScopeConfiguration
		{
			get;
			set;
		}

		public bool Enable
		{
			get;
			set;
		}

		public bool IsDropTable
		{
			get;
			set;
		}

		public bool IsInventorySyncEnabled
		{
			get;
			set;
		}

		public string ClientId
		{
			get;
			set;
		}

		public string ClientSecret
		{
			get;
			set;
		}

		public string CallbackURL
		{
			get;
			set;
		}

		public string SalesAccountCode
		{
			get;
			set;
		}

		public string NonVatAccountCode
		{
			get;
			set;
		}

		public string PurchaseAccountCode
		{
			get;
			set;
		}

		public string PostageAccountCode
		{
			get;
			set;
		}

		public string DiscountAmountAccountCode
		{
			get;
			set;
		}

		public string RewardPointsAccountCode
		{
			get;
			set;
		}

		public int BatchSize
		{
			get;
			set;
		}

		public string AuthorizedURL
		{
			get;
			set;
		}

		public string AuthorizedResponseType
		{
			get;
			set;
		}

		public string AuthorizedState
		{
			get;
			set;
		}

		public string TokenURL
		{
			get;
			set;
		}

		public string TokenGrantType
		{
			get;
			set;
		}

		public string Scope
		{
			get;
			set;
		}

		public string ConnectionsURL
		{
			get;
			set;
		}

		public string PaymentMethodAdditonalFeeAccountCode
		{
			get;
			set;
		}
	}
}