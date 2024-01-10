using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.Customer
{
    public partial record RegisterModel 
    {

        //[JsonProperty("guest")]
        //public bool Guest { get; set; }

        [NopResourceDisplayName("Account.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Account.Fields.LastName")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Account.Fields.Phone")]
        public string Phone { get; set; }
       
        public string DateOfBirth { get; set; }

        [NopResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.Fields.Password")]
        public string Password { get; set; }
        public string DeviceToken { get; set; }
        public string AppVersion { get; set; }
        public string DeviceType { get; set; }
        public string languageId { get; set; }

    }
}