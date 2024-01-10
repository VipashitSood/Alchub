using Nop.Core;

namespace Nop.Plugin.Forefront.Xero.Domain
{
    public class XeroAccounting : BaseEntity
	{
        public XeroAccounting()
        {
        }

        public string NopPaymentMethod
		{
			get;
			set;
		}

		public string XeroAccountId
		{
			get;
			set;
		}
	}
}