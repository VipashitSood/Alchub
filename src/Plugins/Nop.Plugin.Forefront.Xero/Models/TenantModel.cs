using Newtonsoft.Json;
using System;

namespace Nop.Plugin.Forefront.Xero.Models
{
	public class TenantModel
	{
		[JsonProperty("id")]
		public Guid id { get; set; }

		[JsonProperty("tenantId")]
		public Guid TenantId { get; set; }

		[JsonProperty("tenantType")]
		public string TenantType { get; set; }

        [JsonProperty("createdDateUtc")]
        public DateTime? Tenant_CreatedDateUtc { get; set; }

        [JsonProperty("updatedDateUtc")]
        public DateTime? Tenant_UpdatedDateUtc { get; set; }
    }
}