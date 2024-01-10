using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.Customer
{
    public class ForgetPasswordModel
    {
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.PasswordRecovery.Email")]
        public string Email { get; set; }
       
        /// <summary>
        /// Gets or sets languageId
        /// </summary>
        public int LanguageId { get; set; } = 1;
    }
}
