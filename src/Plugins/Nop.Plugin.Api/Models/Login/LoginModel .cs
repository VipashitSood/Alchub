using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Models.Login;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Api.Models.Login
{
   public class LoginModel
    {
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.Login.Fields.Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.Login.Fields.Password")]
        public string Password { get; set; }
        public int LanguageId { get; set; }
        public string DeviceToken { get; set; }
        public string AppVersion { get; set; }
        public string DeviceType { get; set; }
    }

}
