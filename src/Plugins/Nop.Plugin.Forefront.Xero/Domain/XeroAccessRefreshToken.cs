using System;
using Nop.Core;

namespace Nop.Plugin.Forefront.Xero.Domain
{
    public class XeroAccessRefreshToken : BaseEntity
	{
        public XeroAccessRefreshToken()
        {
        }

        public string AccessToken
        {
			get;
			set;
		}

		public string RefreshToken
        {
			get;
			set;
		}

		public string IdentityToken
        {
			get;
			set;
		}

        public DateTime ExpiresIn
        {
            get;
            set;
        }

        public Guid? Tenant_Id
        {
            get;
            set;
        }

        public Guid? Tenant_TenantId
        {
            get;
            set;
        }

        public string Tenant_TenantType
        {
            get;
            set;
        }

        public DateTime? Tenant_CreatedDateUtc
        {
            get;
            set;
        }

        public DateTime? Tenant_UpdatedDateUtc
        {
            get;
            set;
        }
	}
}