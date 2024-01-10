using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTO.Customers
{
    [JsonObject(Title = "ProfileData")]
    public class ProfileDataDto : BaseDto
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("date_of_birth")]
        public string DateOfBirth { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("phonenumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("profilepicurl")]
        public string ProfilePicUrl { get; set; }

    }
}
