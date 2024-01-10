using Newtonsoft.Json;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Api.Models.Address
{
    public class ChangePasswordModel
    {
        [JsonProperty("userid")]
        public int UserId { get; set; }
        [JsonProperty("OldPassword")]
        public string OldPassword { get; set; }
        [JsonProperty("NewPassword")]
        public string NewPassword { get; set; }
        [JsonProperty("ConfirmNewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}
