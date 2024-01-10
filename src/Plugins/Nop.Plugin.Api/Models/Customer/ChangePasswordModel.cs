using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.Customer
{
   public class ResetPasswordModel
    {
        public int Userid { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.ChangePassword.Fields.NewPassword")]
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
