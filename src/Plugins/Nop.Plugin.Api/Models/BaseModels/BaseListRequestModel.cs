namespace Nop.Plugin.Api.Models.BaseModels
{
    public partial record BaseListRequestModel : BaseRequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PageSize { get; set; }
    }
}