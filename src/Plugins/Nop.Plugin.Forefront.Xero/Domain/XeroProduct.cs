using System;
using Nop.Core;

namespace Nop.Plugin.Forefront.Xero.Domain
{
    public class XeroProduct : BaseEntity
	{
        public XeroProduct()
        {
        }

        public string ActionType
		{
			get;
			set;
		}

		public DateTime? InTime
		{
			get;
			set;
		}

		public bool IsDeleted
		{
			get;
			set;
		}

		public int ProductId
		{
			get;
			set;
		}

		public int SyncAttemptCount
		{
			get;
			set;
		}

		public string XeroId
		{
			get;
			set;
		}

		public bool XeroStatus
		{
			get;
			set;
		}

	}
}