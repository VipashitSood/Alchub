using Nop.Plugin.Api.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.Address
{
    public class AddressListModel : BaseSearchModel
    {
        public int Userid { get; set; }
    }
}
