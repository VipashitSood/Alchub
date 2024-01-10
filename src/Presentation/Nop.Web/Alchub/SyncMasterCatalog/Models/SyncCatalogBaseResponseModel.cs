using System.Collections.Generic;

namespace Nop.Web.Alchub.SyncMasterCatalog.Models
{
    public class SyncCatalogBaseResponseModel
    {
        public SyncCatalogBaseResponseModel()
        {
            Errors = new List<string>();
        }

        public int StatusCode { get; set; }

        public string Message { get; set; }

        public List<string> Errors { get; set; }

        public object Data { get; set; }
    }
}
