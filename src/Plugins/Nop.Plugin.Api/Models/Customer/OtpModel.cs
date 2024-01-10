using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.Customer
{
    public class OtpModel
    {
        public int OTP { get; set; }
        public int Userid { get; set; }
    }
}
