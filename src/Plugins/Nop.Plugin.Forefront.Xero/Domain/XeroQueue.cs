using System;
using Nop.Core;

namespace Nop.Plugin.Forefront.Xero.Domain
{
    public class XeroQueue : BaseEntity
	{
        public XeroQueue()
        {
        }

        public string ActionType
		{
			get;
			set;
		}

		public decimal Amount
		{
			get;
			set;
		}

		public bool? IsPaid
		{
			get;
			set;
		}

		public bool IsSuccess
		{
			get;
			set;
		}

		public int OrderId
		{
			get;
			set;
		}

		public int ParentId
		{
			get;
			set;
		}

		public DateTime QueuedOn
		{
			get;
			set;
		}

		public string ResponseData
		{
			get;
			set;
		}

		public string ResponseMessages
		{
			get;
			set;
		}

		public int SyncAttemptCount
		{
			get;
			set;
		}

		public DateTime? SyncAttemptOn
		{
			get;
			set;
		}

		public string XeroId
		{
			get;
			set;
		}
	}
}