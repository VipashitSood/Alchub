using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;

namespace Nop.Plugin.Api.Models.Customer
{
	public partial record ProfileDataModel
    {
        [JsonProperty("UserId")]
        public int UserId { get; set; }

        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonProperty("Gender")]
        public string Gender { get; set; }

        [JsonProperty("PhoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("ProfilePicUrl")]
        public IFormFile ProfilePicUrl { get; set; }

    }
}
