using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.BaseModels
{
    public partial class BaseSearchModel
    {
        ///// <summary>
        ///// TotalRecords
        ///// </summary>
        //public int TotalRecords { get; set; }

        /// <summary>
        /// PageIndex
        /// </summary>
        public int? PageIndex { get; set; }

        /// <summary>
        /// PageSize
        /// </summary>
        public int? PageSize { get; set; }
    }
}
