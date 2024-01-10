using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.Customer
{
    public partial record UserProfileModel
    {
        [NopResourceDisplayName("Account.Fields.UserId")]
        public int UserId { get; set; }

        [NopResourceDisplayName("Account.Fields.Token")]
        public string Token { get; set; }
    }
}
